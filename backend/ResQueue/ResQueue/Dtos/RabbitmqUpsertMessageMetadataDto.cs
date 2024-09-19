namespace ResQueue.Dtos;

public class RabbitmqUpsertMessageMetadataDto
{
    public required string RoutingKey { get; set; }
    public required RabbitmqMessagePropertiesDto Properties { get; set; }
}