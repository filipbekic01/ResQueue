using Resqueue.Enums;

namespace Resqueue.Dtos;

public class BrokerAccessDto
{
    /// <summary>
    /// The ID of the user to grant or update access for.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// The access level to assign to the user. Null is to remove access level.
    /// </summary>
    public AccessLevel? AccessLevel { get; set; }
}