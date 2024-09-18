using Resqueue.Dtos.Broker;
using Resqueue.Models;

namespace Resqueue.Dtos;

public record BrokerDto(
    string Id,
    List<BrokerAccessDto> AccessList,
    string System,
    string Name,
    RabbitMQConnectionDto? RabbitMQConnection,
    BrokerSettingsDto Settings,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? SyncedAt,
    DateTime? DeletedAt
);