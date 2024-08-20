namespace Resqueue.Models;

public class RabbitmqMessageMetadata
{
    public required bool Redelivered { get; set; }
    public required string Exchange { get; set; }
    public required string RoutingKey { get; set; }
    public required RabbitmqMessageProperties Properties { get; set; }
}