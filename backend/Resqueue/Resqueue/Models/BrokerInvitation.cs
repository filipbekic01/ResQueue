using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Resqueue.Enums;

namespace Resqueue.Models;

public class BrokerInvitation
{
    [BsonId] public ObjectId Id { get; set; }
    public ObjectId BrokerId { get; set; }
    public ObjectId UserId { get; set; }
    public string Token { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; } = false;
}