using System.Text.Json.Nodes;

namespace Resqueue.Dtos;

public class MessageDto
{
    public string Id { get; set; }
    public JsonNode Body { get; set; }

    /// <summary>
    ///  "json" or "base64"
    /// </summary>
    public string BodyEncoding { get; set; }

    public RabbitmqMessageMetadataDto? RabbitmqMetadata { get; set; }
    public string Summary { get; set; }
    public bool IsReviewed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}