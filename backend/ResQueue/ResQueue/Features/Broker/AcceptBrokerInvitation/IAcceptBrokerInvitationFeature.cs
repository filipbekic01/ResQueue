namespace ResQueue.Features.Broker.AcceptBrokerInvitation;

public interface IAcceptBrokerInvitationFeature
{
    public Task<OperationResult<AcceptBrokerInvitationResponse>> ExecuteAsync(
        AcceptBrokerInvitationRequest request);
}