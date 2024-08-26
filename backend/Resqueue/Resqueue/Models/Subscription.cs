namespace Resqueue.Models;

public class Subscription
{
    public string Type { get; set; } = null!;
    public string StripeId { get; set; } = null!; // Unique
    public string StripeStatus { get; set; } = null!;
    public string StripePrice { get; set; } = null!;
    public long Quantity { get; set; } = 0;
    public DateTime? EndsAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<SubscriptionItem> SubscriptionItems { get; set; } = new();
}