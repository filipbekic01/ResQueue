using System.Text.Json.Nodes;

namespace ResQueue.Dtos;

public class UpsertMessageDto
{
    public string BrokerId { get; set; } = null!;
    public string QueueId { get; set; } = null!;
    public JsonNode Body { get; set; } = null!;

    /// <summary>
    ///  "json" or "base64"
    /// </summary>
    public string BodyEncoding { get; set; } = null!;

    public RabbitmqUpsertMessageMetadataDto? RabbitmqMetadata { get; set; }
}