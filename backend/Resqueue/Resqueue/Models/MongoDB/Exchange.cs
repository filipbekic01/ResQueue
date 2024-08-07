using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Resqueue.Models.MongoDB;

public class Exchange
{
    [BsonId] public ObjectId Id { get; set; }
    public ObjectId BrokerId { get; set; }
    public string UserId { get; set; }
    public BsonDocument RawData { get; set; }
}