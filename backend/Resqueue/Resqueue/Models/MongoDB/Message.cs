using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Resqueue.Models.MongoDB;

public class Message
{
    [BsonId] public ObjectId Id { get; set; }
    public ObjectId QueueId { get; set; }
    public BsonDocument RawData { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime DeletedAt { get; set; }
}