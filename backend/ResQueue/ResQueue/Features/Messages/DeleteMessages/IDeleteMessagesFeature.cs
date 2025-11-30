namespace ResQueue.Features.Messages.DeleteMessages;

public interface IDeleteMessagesFeature
{
    Task<DeleteMessagesResponse> ExecuteAsync(DeleteMessagesRequest request);
}