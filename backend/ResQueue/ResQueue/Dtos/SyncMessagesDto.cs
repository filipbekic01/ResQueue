namespace ResQueue.Dtos;

public record SyncMessagesDto(
    string BrokerId,
    string QueueId
);