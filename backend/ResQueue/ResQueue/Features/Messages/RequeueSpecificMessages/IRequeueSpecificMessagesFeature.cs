namespace ResQueue.Features.Messages.MoveMessage;

public interface IRequeueSpecificMessagesFeature
{
    Task<OperationResult<RequeueSpecificMessagesResponse>> ExecuteAsync(RequeueSpecificMessagesRequest request);
}