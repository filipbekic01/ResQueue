using System.Security.Claims;
using Marten;
using Marten.Patching;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ResQueue.Models;
using Stripe;

namespace ResQueue.Features.Stripe.ContinueSubscription;

public record ContinueSubscriptionRequest(
    ClaimsPrincipal ClaimsPrincipal
);

public record ContinueSubscriptionResponse();

public class ContinueSubscriptionFeature(
    IOptions<Settings> settings,
    IDocumentSession documentSession,
    UserManager<User> userManager
) : IContinueSubscriptionFeature
{
    public async Task<OperationResult<ContinueSubscriptionResponse>> ExecuteAsync(ContinueSubscriptionRequest request)
    {
        var user = await userManager.GetUserAsync(request.ClaimsPrincipal);
        if (user is null)
        {
            return OperationResult<ContinueSubscriptionResponse>.Failure(new ProblemDetails
            {
                Title = "Unauthorized Access",
                Detail = "The user could not be found or is not logged in.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        if (user.Subscription is null)
        {
            return OperationResult<ContinueSubscriptionResponse>.Failure(new ProblemDetails
            {
                Title = "Invalid Subscription",
                Detail = "No active subscription was found for the user.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var stripeClient = new StripeClient(settings.Value.StripeSecret);

            var subscription = await stripeClient.V1.Subscriptions.GetAsync(user.Subscription.StripeId);

            if (subscription.Status == "canceled")
            {
                return OperationResult<ContinueSubscriptionResponse>.Failure(new ProblemDetails
                {
                    Title = "Subscription Ended",
                    Detail = "The subscription has already ended and cannot be reactivated.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (subscription.CancelAtPeriodEnd)
            {
                var updatedSubscription = await stripeClient.V1.Subscriptions.UpdateAsync(user.Subscription.StripeId,
                    new SubscriptionUpdateOptions
                    {
                        CancelAtPeriodEnd = false
                    });

                user.Subscription.StripeStatus = updatedSubscription.Status;
                user.Subscription.EndsAt = null;

                documentSession.Patch<User>(user.Id)
                    .Set(x => x.Subscription, user.Subscription);

                await documentSession.SaveChangesAsync();

                return OperationResult<ContinueSubscriptionResponse>.Success(new ContinueSubscriptionResponse());
            }

            return OperationResult<ContinueSubscriptionResponse>.Failure(new ProblemDetails
            {
                Title = "Subscription Not Set to Cancel",
                Detail = "The subscription is not set to cancel at the end of the period.",
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (StripeException e)
        {
            return OperationResult<ContinueSubscriptionResponse>.Failure(new ProblemDetails
            {
                Title = "Stripe Error",
                Detail = $"An error occurred while updating the subscription: {e.Message}",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}