using System.Security.Claims;

namespace Resqueue.Features.Broker.UpdateBroker;

public interface IUpdateBrokerFeature
{
    Task<OperationResult<UpdateBrokerFeatureResponse>> ExecuteAsync(UpdateBrokerFeatureRequest request);
}