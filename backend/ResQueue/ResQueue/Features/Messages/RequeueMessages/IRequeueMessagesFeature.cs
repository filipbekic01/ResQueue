namespace ResQueue.Features.Messages.RequeueMessages;

public interface IRequeueMessagesFeature
{
    Task<RequeueMessagesResponse> ExecuteAsync(RequeueMessagesRequest request);
}