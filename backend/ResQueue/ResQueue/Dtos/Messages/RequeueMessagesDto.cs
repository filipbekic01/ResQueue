namespace ResQueue.Dtos;

public record RequeueMessagesDto(
    string QueueName,
    int SourceQueueType,
    int TargetQueueType,
    int MessageCount,
    int RedeliveryCount
);