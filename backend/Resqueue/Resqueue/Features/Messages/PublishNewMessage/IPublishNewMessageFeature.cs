namespace Resqueue.Features.Messages.PublishNewMessage;

public interface IPublishNewMessageFeature
{
    Task<OperationResult<PublishNewMessageFeatureResponse>> ExecuteAsync(PublishNewMessageFeatureRequest request);
}