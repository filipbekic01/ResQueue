namespace Resqueue.Dtos;

public class UserDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }

    public string? StripeId { get; set; }
    public string? PaymentType { get; set; }
    public string? PaymentLastFour { get; set; }

    public UserConfigDto UserConfig { get; set; }
    public List<SubscriptionDto> Subscriptions { get; set; } = new();
}