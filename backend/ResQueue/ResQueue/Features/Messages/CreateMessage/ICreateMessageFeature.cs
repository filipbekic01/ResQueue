namespace ResQueue.Features.Messages.CreateMessage;

public interface ICreateMessageFeature
{
    Task<OperationResult<CreateMessageFeatureResponse>> ExecuteAsync(CreateMessageFeatureRequest request);
}