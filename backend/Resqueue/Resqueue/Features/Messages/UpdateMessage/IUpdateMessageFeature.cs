using Resqueue.Features.Messages.UpdateMessage;

namespace Resqueue.Features.Messages.CreateMessage;

public interface IUpdateMessageFeature
{
    Task<OperationResult<UpdateMessageFeatureResponse>> ExecuteAsync(UpdateMessageFeatureRequest request);
}