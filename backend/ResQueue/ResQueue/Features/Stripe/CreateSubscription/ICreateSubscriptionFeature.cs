namespace ResQueue.Features.Stripe.CreateSubscription;

public interface ICreateSubscriptionFeature
{
    Task<OperationResult<CreateSubscriptionResponse>> ExecuteAsync(CreateSubscriptionRequest request);
}