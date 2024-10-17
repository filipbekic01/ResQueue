using ResQueue.Constants;
using ResQueue.Dtos;
using ResQueue.Enums;
using ResQueue.Models;

namespace ResQueue.Mappers;

public static class CreateBrokerDtoMapper
{
    public static Broker ToBroker(string userId, CreateBrokerDto dto)
    {
        var dateTime = DateTime.UtcNow;

        return new Broker
        {
            AccessList =
            [
                new BrokerAccess
                {
                    UserId = userId,
                    AccessLevel = AccessLevel.Owner
                }
            ],
            CreatedByUserId = userId,
            System = BrokerSystems.POSTGRES,
            Name = dto.Name,
            PostgresConnection = dto.PostgresConnection is { } postgresConnection
                ? new PostgresConnection(
                    Host: postgresConnection.Host,
                    Username: postgresConnection.Username,
                    Password: postgresConnection.Password,
                    Database: postgresConnection.Database,
                    Port: postgresConnection.Port
                )
                : null,
            CreatedAt = dateTime,
            UpdatedAt = dateTime
        };
    }
}