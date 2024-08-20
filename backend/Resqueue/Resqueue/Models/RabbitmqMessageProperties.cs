using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Resqueue.Models;

public class RabbitmqMessageProperties
{
    // [BsonIgnoreIfDefault] public bool IsAppIdPresent { get; set; }

    [BsonIgnoreIfNull] public string? AppId { get; set; }

    // [BsonIgnoreIfDefault] public bool IsClusterIdPresent { get; set; }
    [BsonIgnoreIfNull] public string? ClusterId { get; set; }

    // [BsonIgnoreIfDefault] public bool IsContentEncodingPresent { get; set; }
    [BsonIgnoreIfNull] public string? ContentEncoding { get; set; }

    // [BsonIgnoreIfDefault] public bool IsContentTypePresent { get; set; }
    [BsonIgnoreIfNull] public string? ContentType { get; set; }

    // [BsonIgnoreIfDefault] public bool IsCorrelationIdPresent { get; set; }
    [BsonIgnoreIfNull] public string? CorrelationId { get; set; }

    // [BsonIgnoreIfDefault] public bool IsDeliveryModePresent { get; set; }
    [BsonIgnoreIfNull] public byte? DeliveryMode { get; set; }

    // [BsonIgnoreIfDefault] public bool IsExpirationPresent { get; set; }
    [BsonIgnoreIfNull] public string? Expiration { get; set; }

    // [BsonIgnoreIfDefault] public bool IsHeaderPresent { get; set; }
    //
    // [BsonDictionaryOptions(DictionaryRepresentation.Document)]
    // [BsonIgnoreIfNull]
    // public IDictionary<string, object>? Headers { get; set; }
    [BsonIgnoreIfNull] public BsonDocument? Headers { get; set; }

    // [BsonIgnoreIfDefault] public bool IsMessageIdPresent { get; set; }
    [BsonIgnoreIfNull] public string? MessageId { get; set; }

    // [BsonIgnoreIfDefault] public bool IsPriorityPresent { get; set; }
    [BsonIgnoreIfNull] public byte? Priority { get; set; }

    // [BsonIgnoreIfDefault] public bool IsReplyToPresent { get; set; }
    [BsonIgnoreIfNull] public string? ReplyTo { get; set; }

    // [BsonIgnoreIfDefault] public bool IsTimestampPresent { get; set; }
    [BsonIgnoreIfNull] public long? Timestamp { get; set; }

    // [BsonIgnoreIfDefault] public bool IsTypePresent { get; set; }
    [BsonIgnoreIfNull] public string? Type { get; set; }

    // [BsonIgnoreIfDefault] public bool IsUserIdPresent { get; set; }
    [BsonIgnoreIfNull] public string? UserId { get; set; }
}