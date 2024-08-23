namespace Resqueue.Dtos;

public class UserDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public bool IsSubscribed { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? SubscriptionId { get; set; }
    public string? SubscriptionPlan { get; set; }
    public UserConfigDto? UserConfig { get; set; }
}