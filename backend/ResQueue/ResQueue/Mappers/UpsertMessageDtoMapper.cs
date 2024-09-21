using System.Text;
using System.Text.Json;
using MongoDB.Bson;
using ResQueue.Dtos;
using ResQueue.Models;

namespace ResQueue.Mappers;

public static class UpsertMessageDtoMapper
{
    public static Message ToMessage(ObjectId queueId, ObjectId userId, long messageOrder, UpsertMessageDto dto)
    {
        return new Message()
        {
            QueueId = queueId,
            UserId = userId,
            RabbitMQMeta = dto.RabbitmqMetadata is not null
                ? new RabbitMQMessageMeta
                {
                    Redelivered = false,
                    Exchange = null,
                    RoutingKey = dto.RabbitmqMetadata.RoutingKey,
                    Properties =
                        new RabbitMQMessageProperties
                        {
                            AppId = dto.RabbitmqMetadata.Properties.AppId,
                            ClusterId = dto.RabbitmqMetadata.Properties.ClusterId,
                            ContentEncoding = dto.RabbitmqMetadata.Properties.ContentEncoding,
                            ContentType = dto.RabbitmqMetadata.Properties.ContentType,
                            CorrelationId = dto.RabbitmqMetadata.Properties.CorrelationId,
                            DeliveryMode = dto.RabbitmqMetadata.Properties.DeliveryMode,
                            Expiration = dto.RabbitmqMetadata.Properties.Expiration,
                            Headers = GetHeaders(dto),
                            MessageId = dto.RabbitmqMetadata.Properties.MessageId,
                            Priority = dto.RabbitmqMetadata.Properties.Priority,
                            ReplyTo = dto.RabbitmqMetadata.Properties.ReplyTo,
                            Timestamp = dto.RabbitmqMetadata.Properties.Timestamp,
                            Type = dto.RabbitmqMetadata.Properties.Type,
                            UserId = dto.RabbitmqMetadata.Properties.UserId,
                        }
                }
                : null,
            Body = GetBody(dto),
            MessageOrder = messageOrder,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static BsonValue GetBody(UpsertMessageDto dto)
        => dto.BodyEncoding switch
        {
            "json" => BsonDocument.Parse(dto.Body.ToJsonString()),
            "string" => dto.Body.ToString(),
            "base64" => new BsonBinaryData(Convert.FromBase64String(dto.Body.ToString())),
            _ => throw new Exception($"Unknown encoding: {dto.BodyEncoding}"),
        };

    private static IDictionary<string, object>? GetHeaders(UpsertMessageDto dto)
    {
        return dto.RabbitmqMetadata?.Properties.Headers?.ToDictionary(x => x.Key, x =>
            {
                if (x.Value is JsonElement element)
                {
                    return ToMongoFriendlyObject(element);
                }

                return x.Value;
            }).Where(x => x.Value is not null)
            .ToDictionary(x => x.Key, x => x.Value ?? throw new NullReferenceException());
    }

    private static object? ToMongoFriendlyObject(JsonElement element)
        => element.ValueKind switch
        {
            JsonValueKind.False or JsonValueKind.True => element.GetBoolean(),
            JsonValueKind.Number => element.TryGetInt32(out var int32) ? int32 :
                element.TryGetInt64(out var int64) ? int64 : element.GetDouble(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.Array => element.EnumerateArray().Select(ToMongoFriendlyObject).ToList(),
            _ => throw new Exception($"Unexpected JsonValueKind {element.ValueKind}"),
        };
}