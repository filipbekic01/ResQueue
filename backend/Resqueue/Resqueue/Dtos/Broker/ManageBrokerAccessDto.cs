using Resqueue.Enums;

namespace Resqueue.Dtos.Broker;

public class ManageBrokerAccessDto
{
    public string UserId { get; set; }
    public AccessLevel? AccessLevel { get; set; }
}