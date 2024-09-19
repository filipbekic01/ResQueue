namespace ResQueue.Features.Stripe.EventHandler;

public interface IEventHandlerFeature
{
    Task<OperationResult<EventHandlerResponse>> ExecuteAsync(EventHandlerRequest request);
}