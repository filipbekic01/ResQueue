using Resqueue.Dtos;
using Resqueue.Dtos.Broker;
using Resqueue.Models;

namespace Resqueue.Mappers;

public class BrokerMapper
{
    public static BrokerDto ToDto(Broker broker)
    {
        return new BrokerDto(
            Id: broker.Id.ToString(),
            System: broker.System,
            Name: broker.Name,
            RabbitMQConnection: broker.RabbitMQConnection is { } rabbitMqConnection
                ? new(
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
                AccessLevel = y.AccessLevel
            }).ToList(),
            Settings: new BrokerSettingsDto(
                QuickSearches: broker.Settings.QuickSearches,
                DeadLetterQueueSuffix: broker.Settings.DeadLetterQueueSuffix,
                MessageFormat: broker.Settings.MessageFormat,
                MessageStructure: broker.Settings.MessageStructure,
                QueueTrimPrefix: broker.Settings.QueueTrimPrefix,
                DefaultQueueSortField: broker.Settings.DefaultQueueSortField,
                DefaultQueueSortOrder: broker.Settings.DefaultQueueSortOrder,
                DefaultQueueSearch: broker.Settings.DefaultQueueSearch
            ),
            CreatedAt: broker.CreatedAt,
            UpdatedAt: broker.UpdatedAt,
            SyncedAt: broker.SyncedAt,
            DeletedAt: broker.DeletedAt
        );
    }
}