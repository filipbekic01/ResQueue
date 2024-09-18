namespace Resqueue.Dtos.Broker;

public class BrokerInvitationDto
{
    public string Id { get; set; }
    public string BrokerId { get; set; }
    public string InviterId { get; set; }
    public string InviteeId { get; set; }
    public string InviterEmail { get; set; }
    public string Token { get; set; }
    public string BrokerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; }
}