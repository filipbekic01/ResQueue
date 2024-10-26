namespace ResQueue.Features.Messages.DeleteMessages;

public interface IDeleteMessagesFeature
{
    Task<OperationResult<DeleteMessagesResponse>> ExecuteAsync(DeleteMessagesRequest request);
}