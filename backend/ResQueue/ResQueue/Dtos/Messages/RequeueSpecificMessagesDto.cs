namespace ResQueue.Dtos.Messages;

public record RequeueSpecificMessagesDto(
    long[] MessageDeliveryIds,
    int TargetQueueType,
    int Delay,
    int RedeliveryCount,
    bool Transactional
);