namespace ResQueue.Models;

public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; }
    public string QueueId { get; set; }
    public required string Body { get; set; }
    public RabbitMQMessageMeta? RabbitMQMeta { get; set; }
    public bool IsReviewed { get; set; } = false;
    public required long MessageOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}