using System.Text.Json.Nodes;

namespace Resqueue.Dtos;

public class NewMessageDto
{
    public string BrokerId { get; set; }
    public JsonNode Body { get; set; }

    /// <summary>
    ///  "json" or "base64"
    /// </summary>
    public string BodyEncoding { get; set; }

    public RabbitmqNewMessageMetadataDto? RabbitmqMetadata { get; set; }
}