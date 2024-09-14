namespace Resqueue.Dtos;

public class RabbitmqCreateMessageMetadataDto
{
    public required string RoutingKey { get; set; }
    public required RabbitmqMessagePropertiesDto Properties { get; set; }
}