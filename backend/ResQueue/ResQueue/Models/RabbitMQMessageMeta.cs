namespace ResQueue.Models;

public class RabbitMQMessageMeta
{
    public required bool Redelivered { get; set; }
    public required string? Exchange { get; set; }
    public required string RoutingKey { get; set; }
    public required RabbitMQMessageProperties Properties { get; set; }
}