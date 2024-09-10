namespace Resqueue.Dtos;

public record BrokerDto(
    string Id,
    string System,
    string Name,
    int Port,
    string Host,
    string VHost,
    BrokerSettingsDto Settings,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? SyncedAt,
    DateTime? DeletedAt
);