namespace ResQueue.Features.Subscriptions.GetSubscriptions;

public interface IGetSubscriptionsFeature
{
    Task<OperationResult<GetSubscriptionsResponse>> ExecuteAsync(GetSubscriptionsRequest request);
}