namespace Resqueue.Dtos;

public record CreateBrokerDto(
    string Name,
    CreateRabbitMQConnectionDto? RabbitMQConnection
);