namespace Resqueue.Features.Broker.ManageBrokerAccess;

public interface IManageBrokerAccessFeature
{
    Task<OperationResult<ManageBrokerAccessFeatureResponse>> ExecuteAsync(ManageBrokerAccessFeatureRequest request);
}