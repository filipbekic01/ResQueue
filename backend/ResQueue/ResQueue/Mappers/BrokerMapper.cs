using ResQueue.Dtos;
using ResQueue.Dtos.Broker;
using ResQueue.Models;

namespace ResQueue.Mappers;

public class BrokerMapper
{
    public static BrokerDto ToDto(Broker broker)
    {
        return new BrokerDto(
            Id: broker.Id.ToString(),
            System: broker.System,
            Name: broker.Name,
            RabbitMQConnection: broker.RabbitMQConnection is { } rabbitMqConnection
                ? new RabbitMQConnectionDto(
                    ManagementPort: rabbitMqConnection.ManagementPort,
                    ManagementTls: rabbitMqConnection.ManagementTls,
                    AmqpPort: rabbitMqConnection.AmqpPort,
                    AmqpTls: rabbitMqConnection.AmqpTls,
                    Host: rabbitMqConnection.Host,
                    VHost: rabbitMqConnection.VHost
                )
                : null,
            AccessList: broker.AccessList.Select(y => new BrokerAccessDto()
            {
                UserId = y.UserId.ToString(),
                AccessLevel = y.AccessLevel,
                Settings = new BrokerSettingsDto(
                    QuickSearches: y.Settings.QuickSearches,
                    DeadLetterQueueSuffix: y.Settings.DeadLetterQueueSuffix,
                    MessageFormat: y.Settings.MessageFormat,
                    MessageStructure: y.Settings.MessageStructure,
                    QueueTrimPrefix: y.Settings.QueueTrimPrefix,
                    DefaultQueueSortField: y.Settings.DefaultQueueSortField,
                    DefaultQueueSortOrder: y.Settings.DefaultQueueSortOrder,
                    DefaultQueueSearch: y.Settings.DefaultQueueSearch
                )
            }).ToList(),
            CreatedAt: broker.CreatedAt,
            UpdatedAt: broker.UpdatedAt,
            SyncedAt: broker.SyncedAt,
            DeletedAt: broker.DeletedAt
        );
    }
}