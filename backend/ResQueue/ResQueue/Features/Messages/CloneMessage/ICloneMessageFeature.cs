namespace ResQueue.Features.Messages.CloneMessage;

public interface ICloneMessageFeature
{
    Task<OperationResult<CloneMessagesFeatureResponse>> ExecuteAsync(CloneMessagesFeatureRequest request);
}