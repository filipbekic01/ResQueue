namespace Resqueue.Dtos;

public record RabbitMQConnectionDto(
    int ManagementPort,
    bool ManagementTls,
    int AmqpPort,
    bool AmqpTls,
    string Host,
    string VHost
);