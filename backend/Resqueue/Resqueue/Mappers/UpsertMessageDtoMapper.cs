using System.Text;
using MongoDB.Bson;
using Resqueue.Dtos;
using Resqueue.Models;

namespace Resqueue.Mappers;

public static class UpsertMessageDtoMapper
{
    public static Message ToMessage(ObjectId queueId, ObjectId userId, UpsertMessageDto dto)
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
            CreatedAt = DateTime.UtcNow
        };
    }


    private static BsonValue GetBody(UpsertMessageDto dto)
    {
        if (dto.BodyEncoding == "json")
        {
            return BsonDocument.Parse(dto.Body.ToJsonString());
        }

        return new BsonBinaryData(Encoding.UTF8.GetBytes(dto.Body.ToString()));
    }
}