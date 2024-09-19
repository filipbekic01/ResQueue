namespace ResQueue.Features.Stripe.CancelSubscription;

public interface ICancelSubscriptionFeature
{
    Task<OperationResult<CancelSubscriptionResponse>> ExecuteAsync(CancelSubscriptionRequest request);
}