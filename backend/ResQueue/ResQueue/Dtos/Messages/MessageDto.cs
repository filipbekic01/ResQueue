namespace ResQueue.Dtos.Messages;

public class MessageDto
{
    public Guid TransportMessageId { get; set; }
    public string? ContentType { get; set; }
    public string? MessageType { get; set; }
    public string? Body { get; set; }
    public byte[]? BinaryBody { get; set; }
    public Guid? MessageId { get; set; }
    public Guid? CorrelationId { get; set; }
    public Guid? ConversationId { get; set; }
    public Guid? RequestId { get; set; }
    public Guid? InitiatorId { get; set; }
    public Guid? SchedulingTokenId { get; set; }
    public string? SourceAddress { get; set; }
    public string? DestinationAddress { get; set; }
    public string? ResponseAddress { get; set; }
    public string? FaultAddress { get; set; }
    public string? SentTime { get; set; }
    public string? Headers { get; set; }
    public string? Host { get; set; }
}