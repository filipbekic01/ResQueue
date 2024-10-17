namespace ResQueue.Dtos;

public record MoveMessagesDto(
    long[] MessageDeliveryId,
    string QueueName,
    int QueueType
);