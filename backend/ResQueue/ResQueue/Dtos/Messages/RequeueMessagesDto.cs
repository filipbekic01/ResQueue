namespace ResQueue.Dtos.Messages;

public record RequeueMessagesDto(
    string QueueName,
    int SourceQueueType,
    int TargetQueueType,
    int MessageCount,
    int Delay,
    int RedeliveryCount
);