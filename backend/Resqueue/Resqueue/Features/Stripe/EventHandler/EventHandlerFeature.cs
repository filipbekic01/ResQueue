using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Models;
using Resueue.Extensions;
using Stripe;
using Subscription = Stripe.Subscription;

namespace Resqueue.Features.Stripe.EventHandler;

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
                        Detail = "Unauthorized",
                        Status = StatusCodes.Status401Unauthorized
                    });
                }

                var userSub = user.Subscriptions.FirstOrDefault(x => x.StripeId == subscription.Id);
                if (userSub is null)
                {
                    return OperationResult<EventHandlerResponse>.Failure(new ProblemDetails
                    {
                        Detail = "Invalid subscription",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                userSub.StripeStatus = subscription.Status;

                var filter = Builders<User>.Filter.Eq(q => q.Id, user.Id);
                var update = Builders<User>.Update
                    .Set(q => q.Subscriptions, user.Subscriptions);

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
            return OperationResult<EventHandlerResponse>.Failure(new ProblemDetails()
            {
                Detail = e.Message
            });
        }
    }
}