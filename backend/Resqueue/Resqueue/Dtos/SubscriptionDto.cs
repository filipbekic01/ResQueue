namespace Resqueue.Dtos;

public class SubscriptionDto
{
    public string Type { get; set; } = null!;
    public string StripeId { get; set; } = null!; // Unique
    public string StripeStatus { get; set; } = null!;
    public string StripePrice { get; set; } = null!;
    public long Quantity { get; set; } = 0;
    public DateTime? TrialEndsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<SubscriptionItemDto> SubscriptionItems { get; set; } = new();
}