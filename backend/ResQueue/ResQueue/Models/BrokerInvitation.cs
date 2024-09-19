using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ResQueue.Models;

public class BrokerInvitation
{
    [BsonId] public ObjectId Id { get; set; }
    public ObjectId BrokerId { get; set; }
    public ObjectId InviterId { get; set; } // User who invited
    public ObjectId InviteeId { get; set; } // User who was invited
    public string InviterEmail { get; set; }
    public string Token { get; set; }
    public string BrokerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; } = false;
}