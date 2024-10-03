using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ResQueue.Constants;
using ResQueue.Models;
using Stripe;

namespace ResQueue.Features.Stripe.ChangePlan;

public record ChangePlanRequest(
    ClaimsPrincipal ClaimsPrincipal
);

public record ChangePlanResponse();

public class ChangePlanFeature(
    UserManager<User> userManager,
    IMongoCollection<User> usersCollection,
    IOptions<Settings> settings
) : IChangePlanFeature
{
    public async Task<OperationResult<ChangePlanResponse>> ExecuteAsync(ChangePlanRequest request)
    {
        var dt = DateTime.UtcNow;

        var dc = new Dictionary<string, string>
        {
            { StripePlans.ESSENTIALS, settings.Value.StripeEssentialsPriceId },
            { StripePlans.ULTIMATE, settings.Value.StripeUltimatePriceId }
        };

        // Get user
        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user == null || string.IsNullOrEmpty(user.StripeId))
        {
            return OperationResult<ChangePlanResponse>.Failure(new ProblemDetails
            {
                Title = "User Not Found",
                Detail = "The user or subscription could not be found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        if (user.Subscription?.StripeStatus != "active")
        {
            return OperationResult<ChangePlanResponse>.Failure(new ProblemDetails
            {
                Title = "Subscription Not Active",
                Detail = "No active subscription was found for the user.",
                Status = StatusCodes.Status404NotFound
            });
        }

        var newPriceId = user.Subscription.Type == StripePlans.ESSENTIALS
            ? dc.GetValueOrDefault(StripePlans.ULTIMATE)
            : dc.GetValueOrDefault(StripePlans.ESSENTIALS);
        if (newPriceId is null)
        {
            return OperationResult<ChangePlanResponse>.Failure(new ProblemDetails
            {
                Title = "Invalid Price ID",
                Detail = "The new price ID for the subscription plan is invalid.",
                Status = StatusCodes.Status404NotFound
            });
        }

        // Retrieve the subscription
        try
        {
            var stripeClient = new StripeClient(settings.Value.StripeSecret);

            var subscription = await stripeClient.V1.Subscriptions.GetAsync(user.Subscription.StripeId);
            if (subscription == null)
            {
                return OperationResult<ChangePlanResponse>.Failure(new ProblemDetails
                {
                    Title = "Subscription Not Found",
                    Detail = "Active subscription not found on Stripe.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            // Update the subscription with the new plan
            var updatedSubscription = await stripeClient.V1.Subscriptions.UpdateAsync(subscription.Id,
                new SubscriptionUpdateOptions
                {
                    Items =
                    [
                        new SubscriptionItemOptions
                        {
                            Id = subscription.Items.Data[0].Id,
                            Price = newPriceId
                        }
                    ],
                    ProrationBehavior = "create_prorations" // Handle prorations automatically
                });

            // Update MongoDB with the new subscription details
            user.Subscription.Type = user.Subscription.Type == StripePlans.ESSENTIALS
                ? StripePlans.ULTIMATE
                : StripePlans.ESSENTIALS;
            user.Subscription.StripePrice = newPriceId;
            user.Subscription.UpdatedAt = dt;
            user.Subscription.SubscriptionItem.StripePrice = updatedSubscription.Items.First().Price.Id;
            user.Subscription.SubscriptionItem.StripeProduct = updatedSubscription.Items.First().Price.ProductId;
            user.Subscription.SubscriptionItem.UpdatedAt = dt;

            var subscriptionUpdate = Builders<User>.Update
                .Set(q => q.Subscription, user.Subscription);

            var filter = Builders<User>.Filter.Eq(q => q.Id, user.Id);

            await usersCollection.UpdateOneAsync(filter, subscriptionUpdate);

            return OperationResult<ChangePlanResponse>.Success(new ChangePlanResponse());
        }
        catch (StripeException e)
        {
            return OperationResult<ChangePlanResponse>.Failure(new ProblemDetails
            {
                Title = "Stripe Error",
                Detail = $"An error occurred while updating the subscription: {e.Message}",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}