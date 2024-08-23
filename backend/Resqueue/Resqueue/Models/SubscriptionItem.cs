namespace Resqueue.Models;

public class SubscriptionItem
{
    public string StripeId { get; set; } = null!; // Unique
    public string StripeProduct { get; set; } = null!;
    public string StripePrice { get; set; } = null!;
    public long Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}