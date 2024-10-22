namespace ResQueue.Features.Messages.RequeueSpecificMessages;

public interface IRequeueSpecificMessagesFeature
{
    Task<OperationResult<RequeueSpecificMessagesResponse>> ExecuteAsync(RequeueSpecificMessagesRequest request);
}