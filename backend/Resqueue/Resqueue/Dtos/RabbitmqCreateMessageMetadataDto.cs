namespace Resqueue.Dtos;

public class RabbitmqCreateMessageMetadataDto
{
    public required string Exchange { get; set; }
    public required string RoutingKey { get; set; }
    public required RabbitmqMessagePropertiesDto Properties { get; set; }
}