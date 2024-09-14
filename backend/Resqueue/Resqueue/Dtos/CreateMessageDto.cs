using System.Text.Json.Nodes;

namespace Resqueue.Dtos;

public class CreateMessageDto
{
    public string BrokerId { get; set; }
    public string QueueId { get; set; }
    public JsonNode Body { get; set; }

    /// <summary>
    ///  "json" or "base64"
    /// </summary>
    public string BodyEncoding { get; set; }

    public RabbitmqCreateMessageMetadataDto? RabbitmqMetadata { get; set; }
}