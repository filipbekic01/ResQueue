namespace ResQueue.Dtos;

public record PublishDto(
    string ExchangeId,
    string QueueId,
    string BrokerId,
    string[] MessageIds
);