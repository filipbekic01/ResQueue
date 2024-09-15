namespace Resqueue.Dtos;

public record UpdateBrokerDto(
    string Name,
    UpdateRabbitMQConnectionDto RabbitMqConnection,
    BrokerSettingsDto Settings
);