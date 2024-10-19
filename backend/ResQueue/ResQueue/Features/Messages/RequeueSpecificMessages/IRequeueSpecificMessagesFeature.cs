namespace ResQueue.Features.Messages.MoveMessage;

public interface IRequeueSpecificMessagesFeature
{
    Task<OperationResult<MoveMessagesResponse>> ExecuteAsync(MoveMessagesRequest request);
}