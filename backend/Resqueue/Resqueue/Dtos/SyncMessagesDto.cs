namespace Resqueue.Dtos;

public record SyncMessagesDto(
    string BrokerId,
    string QueueId
);