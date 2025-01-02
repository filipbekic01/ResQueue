namespace ResQueue.Dtos.Messages;

public class MessageDeliveryDto
{
    public long MessageDeliveryId { get; set; }
    public Guid TransportMessageId { get; set; }
    public long QueueId { get; set; }
    public short Priority { get; set; }
    public DateTime EnqueueTime { get; set; }
    public DateTime? ExpirationTime { get; set; }
    public string? PartitionKey { get; set; }
    public string? RoutingKey { get; set; }
    public Guid? ConsumerId { get; set; }
    public Guid? LockId { get; set; }
    public int DeliveryCount { get; set; }
    public int MaxDeliveryCount { get; set; }
    public DateTime? LastDelivered { get; set; }
    public string? TransportHeaders { get; set; }

    public MessageDto? Message { get; set; }
    public Dictionary<string, string>? AdditionalData { get; set; }
}