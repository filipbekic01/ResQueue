namespace ResQueue.Dtos;

public record UpdateBrokerDto(
    string Name,
    UpdatePostgresConnectionDto PostgresConnection,
    BrokerSettingsDto Settings
);