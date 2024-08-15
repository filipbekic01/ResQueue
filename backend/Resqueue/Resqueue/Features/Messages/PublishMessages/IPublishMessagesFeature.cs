namespace Resqueue.Features.Messages.PublishMessages;

public interface IPublishMessagesFeature
{
    Task<OperationResult<PublishMessagesFeatureResponse>> ExecuteAsync(PublishMessagesFeatureRequest request);
}