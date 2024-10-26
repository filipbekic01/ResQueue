namespace ResQueue.Dtos.Messages;

public record DeleteMessagesDto(
    DeleteMessagesDto.Message[] Messages,
    bool Transactional
)
{
    public record Message(
        long MessageDeliveryId,
        string LockId
    );
};