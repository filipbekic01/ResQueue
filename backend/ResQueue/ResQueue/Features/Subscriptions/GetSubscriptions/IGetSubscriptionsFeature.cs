namespace ResQueue.Features.Subscriptions.GetSubscriptions;

public interface IGetSubscriptionsFeature
{
    Task<GetSubscriptionsResponse> ExecuteAsync(GetSubscriptionsRequest request);
}