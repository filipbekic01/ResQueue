namespace ResQueue.Dtos;

public record CreateBrokerDto(
    string Name,
    CreateRabbitMQConnectionDto? RabbitMQConnection
);