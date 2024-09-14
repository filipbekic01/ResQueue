namespace Resqueue.Features.Messages.CreateMessage;

public interface ICreateMessageFeature
{
    Task<OperationResult<PublishNewMessageFeatureResponse>> ExecuteAsync(PublishNewMessageFeatureRequest request);
}