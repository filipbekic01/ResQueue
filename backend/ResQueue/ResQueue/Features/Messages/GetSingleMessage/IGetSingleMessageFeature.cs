namespace ResQueue.Features.Messages.GetSingleMessage;

public interface IGetSingleMessageFeature
{
    Task<GetSingleMessageResponse> ExecuteAsync(GetSingleMessageRequest request);
}