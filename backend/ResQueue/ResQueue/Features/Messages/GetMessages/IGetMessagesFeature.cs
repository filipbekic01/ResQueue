namespace ResQueue.Features.Messages.GetMessages;

public interface IGetMessagesFeature
{
    Task<OperationResult<GetMessagesResponse>> ExecuteAsync(GetMessagesRequest request);
}