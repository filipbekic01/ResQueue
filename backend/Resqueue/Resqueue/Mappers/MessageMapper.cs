using System.Text.Json;
using System.Text.Json.Nodes;
using MongoDB.Bson;
using Resqueue.Models;

namespace Resqueue.Dtos;

public static class MessageMapper
{
    public static MessageDto ToDto(Message message)
    {
        JsonNode body;
        string bodyEncoding;
        if (message.Body is BsonDocument)
        {
            body = JsonNode.Parse(message.Body.ToJson()) ?? throw new Exception();
            bodyEncoding = "json";
        }
        else if (message.Body is BsonBinaryData bin)
        {
            body = Convert.ToBase64String(bin.Bytes);
            bodyEncoding = "base64";
        }
        else
        {
            throw new Exception($"Unsupported body type: {message.Body.GetType()}");
        }

        return new MessageDto()
        {
            Id = message.Id.ToString(),
            Body = body,
            BodyEncoding = bodyEncoding,
            RabbitmqMetadata = message.RabbitmqMetadata is not null
                ? MapRabbitmqMetadataToDto(message.RabbitmqMetadata)
                : null,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt,
            IsReviewed = message.IsReviewed
        };
    }

    static RabbitmqMessageMetadataDto MapRabbitmqMetadataToDto(RabbitmqMessageMetadata metadata)
    {
        return new()
        {
            Redelivered = metadata.Redelivered,
            Exchange = metadata.Exchange,
            RoutingKey = metadata.RoutingKey,
            Properties = new()
            {
                AppId = metadata.Properties.AppId,
                ClusterId = metadata.Properties.ClusterId,
                ContentEncoding = metadata.Properties.ContentEncoding,
                ContentType = metadata.Properties.ContentType,
                CorrelationId = metadata.Properties.CorrelationId,
                DeliveryMode = metadata.Properties.DeliveryMode,
                Expiration = metadata.Properties.Expiration,
                Headers = metadata.Properties.Headers is { } headers
                    ? JsonSerializer.Deserialize<IDictionary<string, object>>(headers.ToJson())
                    : null,
                MessageId = metadata.Properties.MessageId,
                Priority = metadata.Properties.Priority,
                ReplyTo = metadata.Properties.ReplyTo,
                Timestamp = metadata.Properties.Timestamp,
                Type = metadata.Properties.Type,
                UserId = metadata.Properties.UserId,
            },
        };
    }
}