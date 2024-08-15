namespace Resqueue.Features.Messages.SyncMessages;

public interface ISyncMessagesFeature
{
    Task<OperationResult<SyncMessagesFeatureResponse>> ExecuteAsync(SyncMessagesFeatureRequest request);
}