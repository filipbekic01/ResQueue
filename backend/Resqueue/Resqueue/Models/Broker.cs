using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Resqueue.Models;

public class Broker
{
    [BsonId] public ObjectId Id { get; set; }
    public ObjectId UserId { get; set; }
    public List<BrokerAccess> AccessList { get; set; } = new();
    public string System { get; set; }
    public string Name { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public int Port { get; set; }
    public string Host { get; set; }
    public string VHost { get; set; }
    public BrokerSettings Settings { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? SyncedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}