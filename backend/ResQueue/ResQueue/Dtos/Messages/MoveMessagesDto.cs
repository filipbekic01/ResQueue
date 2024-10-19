namespace ResQueue.Dtos;

public record RequeueSpecificMessagesDto(
    long[] MessageDeliveryIds,
    int TargetQueueType,
    string Delay,
    int RedeliveryCount,
    bool Transactional
);