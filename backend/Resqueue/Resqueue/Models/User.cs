using AspNetCore.Identity.Mongo.Model;

namespace Resqueue.Models;

public class User : MongoUser
{
    public UserConfig UserConfig { get; set; } = new()
    {
        showBrokerSyncConfirm = true,
        showMessagesSyncConfirm = true
    };
}