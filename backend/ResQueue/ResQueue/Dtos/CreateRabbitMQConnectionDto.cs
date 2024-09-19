namespace ResQueue.Dtos;

public record CreateRabbitMQConnectionDto(
    string Username,
    string Password,
    int ManagementPort,
    bool ManagementTls,
    int AmqpPort,
    bool AmqpTls,
    string Host,
    string VHost
);