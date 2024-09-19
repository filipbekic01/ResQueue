namespace ResQueue.Features.Broker.SyncBroker;

public interface ISyncBrokerFeature
{
    Task<OperationResult<SyncBrokerFeatureResponse>> ExecuteAsync(SyncBrokerFeatureRequest request);
}