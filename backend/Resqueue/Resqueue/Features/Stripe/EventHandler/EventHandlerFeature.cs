using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Resqueue.Features.Stripe.EventHandler;

public record EventHandlerRequest(string JsonBody, string Signature);

public record EventHandlerResponse();

public class EventHandlerFeature : IEventHandlerFeature
{
    public async Task<OperationResult<EventHandlerResponse>> ExecuteAsync(EventHandlerRequest request)
    {
        await Task.Delay(0);

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                request.JsonBody,
                request.Signature,
                "your_webhook_secret"
            );

            // Handle the event
            if (stripeEvent.Type == Events.CustomerSubscriptionCreated)
            {
                var subscription = stripeEvent.Data.Object as Subscription;
                // Process the subscription object
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