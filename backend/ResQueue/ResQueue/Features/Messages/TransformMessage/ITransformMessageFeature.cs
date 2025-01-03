namespace ResQueue.Features.Messages.TransformMessage;

public interface ITransformMessageFeature
{
    Task<OperationResult<TransformMessageResponse>> ExecuteAsync(TransformMessageRequest request);
}