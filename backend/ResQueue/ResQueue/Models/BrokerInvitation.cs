namespace ResQueue.Models;

public class BrokerInvitation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string BrokerId { get; set; }
    public string InviterId { get; set; }
    public string InviteeId { get; set; }
    public string InviterEmail { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string BrokerName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; } = false;
}