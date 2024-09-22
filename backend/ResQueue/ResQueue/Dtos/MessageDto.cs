using System.Text.Json.Nodes;

namespace ResQueue.Dtos;

public class MessageDto
{
    public string Id { get; set; } = null!;
    public JsonNode Body { get; set; } = null!;

    /// <summary>
    ///  "json" or "base64"
    /// </summary>
    public string BodyEncoding { get; set; } = null!;

    public RabbitmqMessageMetadataDto? RabbitmqMetadata { get; set; }
    public bool IsReviewed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}