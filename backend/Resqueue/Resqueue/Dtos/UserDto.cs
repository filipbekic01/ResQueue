namespace Resqueue.Dtos;

public class UserDto
{
    public string Id { get; set; } = null!;
    public string? FullName { get; set; }
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; }

    public string? StripeId { get; set; }
    public string? PaymentType { get; set; }
    public string? PaymentLastFour { get; set; }

    public UserConfigDto UserConfig { get; set; } = null!;
    public List<SubscriptionDto> Subscriptions { get; set; } = [];
}