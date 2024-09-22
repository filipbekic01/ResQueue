namespace ResQueue.Dtos.Broker;

public class BrokerInvitationDto
{
    public string Id { get; set; } = null!;
    public string BrokerId { get; set; } = null!;
    public string InviterId { get; set; } = null!;
    public string InviteeId { get; set; } = null!;
    public string InviterEmail { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string BrokerName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; }
}