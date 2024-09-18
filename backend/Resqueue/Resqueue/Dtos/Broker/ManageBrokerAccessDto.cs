using Resqueue.Enums;

namespace Resqueue.Dtos;

public class ManageBrokerAccessDto
{
    public string UserId { get; set; }
    public AccessLevel? AccessLevel { get; set; }
}