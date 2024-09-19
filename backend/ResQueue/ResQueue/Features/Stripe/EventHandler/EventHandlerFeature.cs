using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ResQueue.Models;
using Stripe;
using Subscription = Stripe.Subscription;

namespace ResQueue.Features.Stripe.EventHandler;

public record EventHandlerRequest(string JsonBody, string Signature);

public record EventHandlerResponse();

public class EventHandlerFeature(
    IOptions<Settings> settings,
    ResqueueUserManager userManager,
    IMongoCollection<User> usersCollection
) : IEventHandlerFeature
{
    public async Task<OperationResult<EventHandlerResponse>> ExecuteAsync(EventHandlerRequest request)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                request.JsonBody,
                request.Signature,
                settings.Value.StripeSecretWebhook
            );

            if (stripeEvent.Type == Events.CustomerSubscriptionDeleted)
            {
                var subscription = stripeEvent.Data.Object as Subscription;

                var user = await userManager.FirstOrDefaultByStripeId(subscription!.CustomerId);
                if (user is null)
                {
                    return OperationResult<EventHandlerResponse>.Failure(new ProblemDetails
                    {
                        Title = "Unauthorized Access",
                        Detail = "The user could not be found or is not authorized.",
                        Status = StatusCodes.Status401Unauthorized
                    });
                }

                if (user.Subscription?.StripeId != subscription.Id)
                {
                    return OperationResult<EventHandlerResponse>.Failure(new ProblemDetails
                    {
                        Title = "Invalid Subscription",
                        Detail = "The subscription does not match the user's records.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                user.Subscription.StripeStatus = subscription.Status;

                var filter = Builders<User>.Filter.Eq(q => q.Id, user.Id);
                var update = Builders<User>.Update
                    .Set(q => q.Subscription, user.Subscription);

                await usersCollection.UpdateOneAsync(filter, update);
            }
            else if (stripeEvent.Type == Events.InvoicePaymentFailed)
            {
                // todo: send e-mail to the user
            }

            return OperationResult<EventHandlerResponse>.Success(new EventHandlerResponse());
        }
        catch (StripeException e)
        {
            return OperationResult<EventHandlerResponse>.Failure(new ProblemDetails
            {
                Title = "Stripe Error",
                Detail = $"An error occurred while processing the event: {e.Message}",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}