namespace ResQueue.Features.Messages.GetSingleMessage;

public interface IGetSingleMessageFeature
{
    Task<OperationResult<GetSingleMessageResponse>> ExecuteAsync(GetSingleMessageRequest request);
}