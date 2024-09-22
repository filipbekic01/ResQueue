using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ResQueue.Models;

public class BrokerInvitation
{
    [BsonId] public ObjectId Id { get; set; }
    public ObjectId BrokerId { get; set; }
    public ObjectId InviterId { get; set; }
    public ObjectId InviteeId { get; set; }
    public string InviterEmail { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string BrokerName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; } = false;
}