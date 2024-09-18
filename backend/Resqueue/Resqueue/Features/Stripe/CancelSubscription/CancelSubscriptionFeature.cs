using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Models;
using Stripe;
using Subscription = Resqueue.Models.Subscription;

namespace Resqueue.Features.Stripe.CancelSubscription;

public record CancelSubscriptionRequest(
    ClaimsPrincipal ClaimsPrincipal
);

public record CancelSubscriptionResponse();

public class CancelSubscriptionFeature(
    IMongoCollection<User> usersCollection,
    IOptions<Settings> settings,
    UserManager<User> userManager
) : ICancelSubscriptionFeature
{
    public async Task<OperationResult<CancelSubscriptionResponse>> ExecuteAsync(CancelSubscriptionRequest request)
    {
        StripeConfiguration.ApiKey = settings.Value.StripeSecret;

        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user is null)
        {
            return OperationResult<CancelSubscriptionResponse>.Failure(new ProblemDetails
            {
                Detail = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        if (user.Subscription is null)
        {
            return OperationResult<CancelSubscriptionResponse>.Failure(new ProblemDetails
            {
                Detail = "Invalid subscription",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var subscriptionService = new SubscriptionService();
            var subscription = await subscriptionService.UpdateAsync(user.Subscription.StripeId,
                new SubscriptionUpdateOptions
                {
                    CancelAtPeriodEnd = true
                });

            user.Subscription.StripeStatus = subscription.Status;
            user.Subscription.EndsAt = subscription.CurrentPeriodEnd;

            var filter = Builders<User>.Filter.Eq(q => q.Id, user.Id);
            var update = Builders<User>.Update.Set(q => q.Subscription, user.Subscription);

            await usersCollection.UpdateOneAsync(filter, update);

            return OperationResult<CancelSubscriptionResponse>.Success(new CancelSubscriptionResponse());
        }
        catch (StripeException e)
        {
            return OperationResult<CancelSubscriptionResponse>.Failure(new ProblemDetails
            {
                Detail = e.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}