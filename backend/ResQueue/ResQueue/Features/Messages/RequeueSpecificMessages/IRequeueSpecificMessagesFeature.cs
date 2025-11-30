namespace ResQueue.Features.Messages.RequeueSpecificMessages;

public interface IRequeueSpecificMessagesFeature
{
    Task<RequeueSpecificMessagesResponse> ExecuteAsync(RequeueSpecificMessagesRequest request);
}