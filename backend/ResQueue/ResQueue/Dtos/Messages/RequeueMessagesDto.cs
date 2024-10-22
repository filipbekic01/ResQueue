namespace ResQueue.Dtos.Messages;

public record RequeueMessagesDto(
    string QueueName,
    int SourceQueueType,
    int TargetQueueType,
    int MessageCount,
    string Delay,
    int RedeliveryCount
);