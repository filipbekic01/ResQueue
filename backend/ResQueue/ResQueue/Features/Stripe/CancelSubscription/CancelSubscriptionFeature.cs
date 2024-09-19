using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ResQueue.Models;
using Stripe;

namespace ResQueue.Features.Stripe.CancelSubscription;

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
                Title = "User Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = "The user associated with the current session could not be located in our system."
            });
        }

        if (user.Subscription is null)
        {
            return OperationResult<CancelSubscriptionResponse>.Failure(new ProblemDetails
            {
                Title = "No Active Subscription",
                Status = StatusCodes.Status400BadRequest,
                Detail = "The user does not have an active subscription to cancel."
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
                Title = "Failed to Update Subscription",
                Status = StatusCodes.Status400BadRequest,
                Detail = $"An error occurred while processing the subscription cancellation: {e.Message}."
            });
        }
    }
}