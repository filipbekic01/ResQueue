namespace ResQueue.Features.Messages.RequeueMessages;

public interface IRequeueMessagesFeature
{
    Task<OperationResult<RequeueMessagesResponse>> ExecuteAsync(RequeueMessagesRequest request);
}