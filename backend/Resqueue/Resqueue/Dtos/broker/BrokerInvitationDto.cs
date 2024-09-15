namespace Resqueue.Dtos;

public class BrokerInvitationDto
{
    public string BrokerId { get; set; }
    public string UserId { get; set; }
    public string Token { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; }
}