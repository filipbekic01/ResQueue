using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ResQueue.Models;

public class Queue
{
    [BsonId] public ObjectId Id { get; set; }
    public ObjectId BrokerId { get; set; }
    public int TotalMessages { get; set; }
    public bool IsFavorite { get; set; }
    public BsonDocument RawData { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public long NextMessageOrder { get; set; }
}