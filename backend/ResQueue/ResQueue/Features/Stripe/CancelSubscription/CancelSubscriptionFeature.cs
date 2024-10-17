using System.Security.Claims;
using Marten;
using Marten.Patching;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ResQueue.Models;
using Stripe;

namespace ResQueue.Features.Stripe.CancelSubscription;

public record CancelSubscriptionRequest(
    ClaimsPrincipal ClaimsPrincipal
);

public record CancelSubscriptionResponse();

public class CancelSubscriptionFeature(
    IDocumentSession documentSession,
    IOptions<Settings> settings,
    UserManager<User> userManager
) : ICancelSubscriptionFeature
{
    public async Task<OperationResult<CancelSubscriptionResponse>> ExecuteAsync(CancelSubscriptionRequest request)
    {
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
            var stripeClient = new StripeClient(settings.Value.StripeSecret);

            var subscription = await stripeClient.V1.Subscriptions.UpdateAsync(user.Subscription.StripeId,
                new SubscriptionUpdateOptions
                {
                    CancelAtPeriodEnd = true
                });

            user.Subscription.StripeStatus = subscription.Status;
            user.Subscription.EndsAt = subscription.CurrentPeriodEnd;

            documentSession.Patch<User>(user.Id)
                .Set(x => x.Subscription, user.Subscription);

            await documentSession.SaveChangesAsync();

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