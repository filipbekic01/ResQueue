using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Resqueue.Models;

public class Message
{
    [BsonId] public ObjectId Id { get; set; }
    public ObjectId UserId { get; set; }
    public ObjectId QueueId { get; set; }
    public required BsonValue Body { get; set; }
    public RabbitmqMessageMetadata? RabbitmqMetadata { get; set; }
    public string Summary { get; set; }
    public bool IsReviewed { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}