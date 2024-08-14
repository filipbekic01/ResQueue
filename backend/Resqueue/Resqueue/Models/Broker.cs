using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Resqueue.Models;

public class Broker
{
    [BsonId] public ObjectId Id { get; set; }
    public ObjectId UserId { get; set; }
    public string System { get; set; }
    public string Name { get; set; }
    public string Auth { get; set; }
    public int Port { get; set; }
    public string Url { get; set; }
    public string Framework { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? SyncedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}