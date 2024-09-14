using System.Text.Json.Nodes;

namespace Resqueue.Dtos;

public class UpsertMessageDto
{
    public string BrokerId { get; set; }
    public string QueueId { get; set; }
    public JsonNode Body { get; set; }

    /// <summary>
    ///  "json" or "base64"
    /// </summary>
    public string BodyEncoding { get; set; }

    public RabbitmqUpsertMessageMetadataDto? RabbitmqMetadata { get; set; }
}