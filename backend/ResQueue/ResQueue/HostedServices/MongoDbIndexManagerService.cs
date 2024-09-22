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

        await CreateBrokerInvitationTtlIndex(collection, cancellationToken);
    }

    private static async Task CreateBrokerInvitationTtlIndex(
        IMongoCollection<BrokerInvitation> collection,
        CancellationToken cancellationToken
    )
    {
        var indexKeysDefinition = Builders<BrokerInvitation>.IndexKeys.Ascending(x => x.ExpiresAt);
        var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero };
        var indexModel = new CreateIndexModel<BrokerInvitation>(indexKeysDefinition, indexOptions);

        await collection.Indexes.CreateOneAsync(indexModel, cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}