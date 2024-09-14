using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Resqueue.Dtos.Stripe;
using Resqueue.Models;
using Stripe;

namespace Resqueue.Features.Stripe.ContinueSubscription;

public record ContinueSubscriptionRequest(
    ClaimsPrincipal ClaimsPrincipal,
    ContinueSubscriptionDto Dto
);

public record ContinueSubscriptionResponse();

public class ContinueSubscriptionFeature(
    IMongoCollection<User> usersCollection,
    IOptions<Settings> settings,
    UserManager<User> userManager
) : IContinueSubscriptionFeature
{
    public async Task<OperationResult<ContinueSubscriptionResponse>> ExecuteAsync(ContinueSubscriptionRequest request)
    {
        StripeConfiguration.ApiKey = settings.Value.StripeSecret;

        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user is null)
        {
            return OperationResult<ContinueSubscriptionResponse>.Failure(new ProblemDetails
            {
                Detail = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var userSub = user.Subscriptions.FirstOrDefault(x => x.StripeId == request.Dto.SubscriptionId);
        if (userSub is null)
        {
            return OperationResult<ContinueSubscriptionResponse>.Failure(new ProblemDetails
            {
                Detail = "Invalid subscription",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var subscriptionService = new SubscriptionService();

            var subscription = await subscriptionService.GetAsync(userSub.StripeId);

            if (subscription.Status == "canceled")
            {
                return OperationResult<ContinueSubscriptionResponse>.Failure(new ProblemDetails
                {
                    Detail = "Subscription has already ended and cannot be reactivated.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (subscription.CancelAtPeriodEnd)
            {
                var updatedSubscription = await subscriptionService.UpdateAsync(userSub.StripeId,
                    new SubscriptionUpdateOptions
                    {
                        CancelAtPeriodEnd = false
                    });

                userSub.StripeStatus = updatedSubscription.Status;
                userSub.EndsAt = null;

                var filter = Builders<User>.Filter.Eq(q => q.Id, user.Id);
                var update = Builders<User>.Update.Set(q => q.Subscriptions, user.Subscriptions);

                await usersCollection.UpdateOneAsync(filter, update);

                return OperationResult<ContinueSubscriptionResponse>.Success(new ContinueSubscriptionResponse());
            }

            return OperationResult<ContinueSubscriptionResponse>.Failure(new ProblemDetails
            {
                Detail = "Subscription is not set to cancel at period end.",
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (StripeException e)
        {
            return OperationResult<ContinueSubscriptionResponse>.Failure(new ProblemDetails
            {
                Detail = e.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}