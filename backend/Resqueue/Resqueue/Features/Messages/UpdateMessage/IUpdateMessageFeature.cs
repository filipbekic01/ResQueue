namespace Resqueue.Features.Messages.UpdateMessage;

public interface IUpdateMessageFeature
{
    Task<OperationResult<UpdateMessageFeatureResponse>> ExecuteAsync(UpdateMessageFeatureRequest request);
}