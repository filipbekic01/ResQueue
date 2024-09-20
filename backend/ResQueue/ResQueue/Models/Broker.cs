using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ResQueue.Models;

public class Broker
{
    [BsonId] public ObjectId Id { get; set; }
    public List<BrokerAccess> AccessList { get; set; } = new();
    public string System { get; set; } = null!;
    public string Name { get; set; } = null!;
    public RabbitMQConnection? RabbitMQConnection { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? SyncedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}