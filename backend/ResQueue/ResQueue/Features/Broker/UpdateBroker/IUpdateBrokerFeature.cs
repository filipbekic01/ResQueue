namespace ResQueue.Features.Broker.UpdateBroker;

public interface IUpdateBrokerFeature
{
    Task<OperationResult<UpdateBrokerFeatureResponse>> ExecuteAsync(UpdateBrokerFeatureRequest request);
}