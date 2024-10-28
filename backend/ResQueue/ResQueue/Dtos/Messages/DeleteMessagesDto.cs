namespace ResQueue.Dtos.Messages;

public record DeleteMessagesDto(
    long[] MessageDeliveryIds,
    bool Transactional
);