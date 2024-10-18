namespace ResQueue.Dtos;

public record MoveMessagesDto(
    MoveMessageDeliveryDto[] Messages,
    string QueueName,
    int QueueType,
    bool Transactional
);

public record MoveMessageDeliveryDto(
    long MessageDeliveryId,
    string LockId,
    string Headers
);