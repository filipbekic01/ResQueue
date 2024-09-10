namespace Resqueue.Dtos;

public record UpdateBrokerDto(
    string Name,
    string Username,
    string Password,
    int Port,
    string Host,
    string VHost,
    BrokerSettingsDto Settings
);