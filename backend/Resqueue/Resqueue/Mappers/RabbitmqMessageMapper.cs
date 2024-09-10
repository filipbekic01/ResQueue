using System.Collections;
using System.Text;
using MongoDB.Bson;
using RabbitMQ.Client;
using Resqueue.Models;

namespace Resqueue.Mappers;

public static class RabbitMQMessageMapper
{
    public static Message ToDocument(ObjectId queueId, ObjectId userId, BasicGetResult res)
    {
        var props = new RabbitMQMessageProperties();

        if (res.BasicProperties.IsAppIdPresent())
        {
            props.AppId = res.BasicProperties.AppId;
        }

        if (res.BasicProperties.IsClusterIdPresent())
        {
            props.ClusterId = res.BasicProperties.ClusterId;
        }

        if (res.BasicProperties.IsContentEncodingPresent())
        {
            props.ContentEncoding = res.BasicProperties.ContentEncoding;
        }

        if (res.BasicProperties.IsContentTypePresent())
        {
            props.ContentType = res.BasicProperties.ContentType;
        }

        if (res.BasicProperties.IsCorrelationIdPresent())
        {
            props.CorrelationId = res.BasicProperties.CorrelationId;
        }

        if (res.BasicProperties.IsDeliveryModePresent())
        {
            props.DeliveryMode = res.BasicProperties.DeliveryMode;
        }

        if (res.BasicProperties.IsExpirationPresent())
        {
            props.Expiration = res.BasicProperties.Expiration;
        }

        if (res.BasicProperties.IsHeadersPresent() && res.BasicProperties.Headers is not null)
        {
            props.Headers = WithBytesConvertedToString(res.BasicProperties.Headers);
        }

        if (res.BasicProperties.IsMessageIdPresent())
        {
            props.MessageId = res.BasicProperties.MessageId;
        }

        if (res.BasicProperties.IsPriorityPresent())
        {
            props.Priority = res.BasicProperties.Priority;
        }

        if (res.BasicProperties.IsReplyToPresent())
        {
            props.ReplyTo = res.BasicProperties.ReplyTo;
        }

        if (res.BasicProperties.IsReplyToPresent())
        {
            props.ReplyTo = res.BasicProperties.ReplyTo;
        }

        if (res.BasicProperties.IsTimestampPresent())
        {
            props.Timestamp = res.BasicProperties.Timestamp.UnixTime;
        }

        if (res.BasicProperties.IsTypePresent())
        {
            props.Type = res.BasicProperties.Type;
        }

        if (res.BasicProperties.IsUserIdPresent())
        {
            props.UserId = res.BasicProperties.UserId;
        }

        return new Message
        {
            QueueId = queueId,
            UserId = userId,
            RabbitMQMeta = new()
            {
                Redelivered = res.Redelivered,
                Exchange = res.Exchange,
                RoutingKey = res.RoutingKey,
                Properties = props
            },
            Body = BsonDocument.TryParse(Encoding.UTF8.GetString(res.Body.Span), out var doc)
                ? doc
                : new BsonBinaryData(res.Body.ToArray()),
            Summary = "Contextual summary of messages",
            CreatedAt = DateTime.UtcNow
        };
    }

    static IDictionary<string, object> WithBytesConvertedToString(IDictionary<string, object> dict)
        => dict.ToDictionary(x => x.Key, x => WithBytesConvertedToString(x.Value));

    static object WithBytesConvertedToString(object value)
        => value switch
        {
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            AmqpTimestamp timestamp => timestamp.ToString(),
            IEnumerable list => list.Cast<object>().Select(WithBytesConvertedToString).ToList(),
            _ => value
        };
}