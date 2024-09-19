namespace ResQueue.Features.Messages.ArchiveMessages;

public interface IArchiveMessagesFeature
{
    Task<OperationResult<ArchiveMessagesFeatureResponse>> ExecuteAsync(ArchiveMessagesFeatureRequest request);
}