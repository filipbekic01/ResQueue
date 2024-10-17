namespace ResQueue.Features.Messages.MoveMessage;

public interface IMoveMessagesFeature
{
    Task<OperationResult<MoveMessagesResponse>> ExecuteAsync(MoveMessagesRequest request);
}