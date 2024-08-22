using AspNetCore.Identity.Mongo.Model;

namespace Resqueue.Models;

public class User : MongoUser
{
    public string SubscriptionPlan { get; set; }
    public string? SubscriptionId { get; set; }
    public bool IsSubscribed { get; set; } = false;

    public UserConfig UserConfig { get; set; } = new()
    {
        showBrokerSyncConfirm = true,
        showMessagesSyncConfirm = true
    };
}