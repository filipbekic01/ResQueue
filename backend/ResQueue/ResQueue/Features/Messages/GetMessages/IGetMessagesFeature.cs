namespace ResQueue.Features.Messages.GetMessages;

public interface IGetMessagesFeature
{
    Task<GetMessagesResponse> ExecuteAsync(GetMessagesRequest request);
}