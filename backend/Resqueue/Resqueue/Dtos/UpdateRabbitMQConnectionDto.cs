namespace Resqueue.Dtos;

public record UpdateRabbitMQConnectionDto(
    string Username,
    string Password,
    int ManagementPort,
    bool ManagementTls,
    int AmqpPort,
    bool AmqpTls,
    string Host,
    string VHost
);