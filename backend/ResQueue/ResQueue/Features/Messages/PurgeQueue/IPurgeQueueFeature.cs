namespace ResQueue.Features.Messages.PurgeQueue;

public interface IPurgeQueueFeature
{
    Task<OperationResult<PurgeQueueResponse>> ExecuteAsync(PurgeQueueRequest request);
}