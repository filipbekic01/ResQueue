using AspNetCore.Identity.Mongo.Model;

namespace Resqueue.Models;

public class User : MongoUser
{
    public string? FullName { get; set; }

    public string? StripeId { get; set; }
    public string? PaymentType { get; set; }
    public string? PaymentLastFour { get; set; }

    public List<Subscription> Subscriptions { get; set; } = [];

    public UserConfig UserConfig { get; set; } = new()
    {
        showBrokerSyncConfirm = true,
        showMessagesSyncConfirm = true
    };
}