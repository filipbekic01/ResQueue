using ResQueue.Dtos.Broker;

namespace ResQueue.Dtos;

public record BrokerDto(
    string Id,
    string CreatedByUserId,
    List<BrokerAccessDto> AccessList,
    string System,
    string Name,
    RabbitMQConnectionDto? RabbitMQConnection,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? SyncedAt,
    DateTime? DeletedAt
);