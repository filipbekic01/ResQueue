namespace ResQueue.Dtos;

public class UserDto
{
    public string Id { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; }

    public string Avatar { get; set; } = null!;
    public string? StripeId { get; set; }
    public string? PaymentType { get; set; }
    public string? PaymentLastFour { get; set; }

    public SubscriptionDto? Subscription { get; set; }

    public UserSettingsDto Settings { get; set; } = null!;
}