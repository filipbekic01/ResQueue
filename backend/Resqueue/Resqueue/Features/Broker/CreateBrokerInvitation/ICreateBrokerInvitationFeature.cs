namespace Resqueue.Features.Broker.CreateBrokerInvitation;

public interface ICreateBrokerInvitationFeature
{
    public Task<OperationResult<CreateBrokerInvitationResponse>> ExecuteAsync(
        CreateBrokerInvitationRequest request);
}