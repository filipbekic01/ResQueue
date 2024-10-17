namespace ResQueue.Dtos;

public record PublishMessagesDto(
    string ExchangeId,
    string QueueId,
    string BrokerId,
    string[] MessageIds
);