using AspNetCore.Identity.Mongo.Model;

namespace ResQueue.Models;

public class User : MongoUser
{
    public string FullName { get; set; } = null!;

    public string Avatar { get; set; } = null!;
    public string? StripeId { get; set; }
    public string? PaymentType { get; set; }
    public string? PaymentLastFour { get; set; }

    public Subscription? Subscription { get; set; }

    public UserSettings Settings { get; set; } = new()
    {
        ShowSyncConfirmDialogs = true,
    };
}