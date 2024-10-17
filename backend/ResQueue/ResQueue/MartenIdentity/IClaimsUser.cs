namespace ResQueue.MartenIdentity;

public interface IClaimsUser
{
    IList<string> RoleClaims { get; set; }
}