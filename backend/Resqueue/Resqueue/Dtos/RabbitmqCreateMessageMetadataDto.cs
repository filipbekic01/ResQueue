namespace Resqueue.Dtos;

public class RabbitmqNewMessageMetadataDto
{
    public required string Exchange { get; set; }
    public required string RoutingKey { get; set; }
    public required RabbitmqMessagePropertiesDto Properties { get; set; }
}