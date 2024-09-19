using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ResQueue.Models;

public class RabbitMQMessageProperties
{
    [BsonIgnoreIfNull] public string? AppId { get; set; }
    [BsonIgnoreIfNull] public string? ClusterId { get; set; }
    [BsonIgnoreIfNull] public string? ContentEncoding { get; set; }
    [BsonIgnoreIfNull] public string? ContentType { get; set; }
    [BsonIgnoreIfNull] public string? CorrelationId { get; set; }
    [BsonIgnoreIfNull] public byte? DeliveryMode { get; set; }
    [BsonIgnoreIfNull] public string? Expiration { get; set; }

    [BsonDictionaryOptions(DictionaryRepresentation.Document)]
    [BsonIgnoreIfNull]
    public IDictionary<string, object>? Headers { get; set; }

    [BsonIgnoreIfNull] public string? MessageId { get; set; }
    [BsonIgnoreIfNull] public byte? Priority { get; set; }
    [BsonIgnoreIfNull] public string? ReplyTo { get; set; }
    [BsonIgnoreIfNull] public long? Timestamp { get; set; }
    [BsonIgnoreIfNull] public string? Type { get; set; }
    [BsonIgnoreIfNull] public string? UserId { get; set; }
}