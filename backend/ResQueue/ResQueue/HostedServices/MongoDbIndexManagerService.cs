using MongoDB.Driver;
using ResQueue.Models;

namespace ResQueue.HostedServices;

public class MongoDbIndexManagerService(
    IServiceProvider serviceProvider
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var collection = scope.ServiceProvider.GetRequiredService<IMongoCollection<BrokerInvitation>>();
        var queuesCollection = scope.ServiceProvider.GetRequiredService<IMongoCollection<Queue>>();

        await CreateBrokerInvitationIndexes(collection, cancellationToken);
        await CreateQueueIndexes(queuesCollection, cancellationToken);
    }

    private static async Task CreateQueueIndexes(
        IMongoCollection<Queue> collection,
        CancellationToken cancellationToken
    )
    {
        // Add index on TotalMessages
        var totalMessagesIndex = Builders<Queue>.IndexKeys.Ascending(q => q.TotalMessages);
        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<Queue>(totalMessagesIndex),
            cancellationToken: cancellationToken
        );

        // Add index on IsFavorite
        var isFavoriteIndex = Builders<Queue>.IndexKeys.Ascending(q => q.IsFavorite);
        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<Queue>(isFavoriteIndex),
            cancellationToken: cancellationToken
        );

        // Add index on BrokerId
        var brokerIdIndex = Builders<Queue>.IndexKeys.Ascending(q => q.BrokerId);
        await collection.Indexes.CreateOneAsync(
            new CreateIndexModel<Queue>(brokerIdIndex),
            cancellationToken: cancellationToken
        );
    }

    private static async Task CreateBrokerInvitationIndexes(
        IMongoCollection<BrokerInvitation> collection,
        CancellationToken cancellationToken
    )
    {
        // Add index on ExpiresAt
        var indexKeysDefinition = Builders<BrokerInvitation>.IndexKeys.Ascending(x => x.ExpiresAt);
        var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero };
        var indexModel = new CreateIndexModel<BrokerInvitation>(indexKeysDefinition, indexOptions);

        await collection.Indexes.CreateOneAsync(indexModel, cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}