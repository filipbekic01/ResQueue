using Microsoft.AspNetCore.Identity;
using ResQueue.MartenIdentity;

namespace ResQueue.Models;

public class User : IdentityUser, IClaimsUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public IList<string> RoleClaims { get; set; }

    public string FullName { get; set; } = null!;

    public string Avatar { get; set; } = null!;
    public string? StripeId { get; set; }
    public string? PaymentType { get; set; }
    public string? PaymentLastFour { get; set; }

    public DateTime CreatedAt { get; set; }

    public Subscription? Subscription { get; set; }


    public UserSettings Settings { get; set; } = new()
    {
        ShowSyncConfirmDialogs = true,
        CollapseSidebar = false
    };
}