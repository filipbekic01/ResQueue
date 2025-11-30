namespace ResQueue.Features.Messages.PurgeQueue;

public interface IPurgeQueueFeature
{
    Task<PurgeQueueResponse> ExecuteAsync(PurgeQueueRequest request);
}