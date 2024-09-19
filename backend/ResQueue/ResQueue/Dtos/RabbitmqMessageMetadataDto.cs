namespace ResQueue.Dtos;

public class RabbitmqMessageMetadataDto
{
    public required bool Redelivered { get; set; }
    public required string? Exchange { get; set; }
    public required string RoutingKey { get; set; }
    public required RabbitmqMessagePropertiesDto Properties { get; set; }
}