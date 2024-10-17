using ResQueue.Enums;

namespace ResQueue.Models;

public class BrokerAccess
{
    public string UserId { get; set; }
    public AccessLevel AccessLevel { get; set; }
    public BrokerSettings Settings { get; set; } = new();
}