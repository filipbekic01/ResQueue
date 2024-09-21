using MongoDB.Driver;
using ResQueue.Models;

namespace ResQueue;

public static class MongoDbExtensions
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, Settings settings)
    {
        services.AddSingleton<IMongoClient, MongoClient>(sp =>
        {
            return new MongoClient(MongoClientSettings.FromConnectionString(settings.MongoDBConnectionString));
        });

        services.AddScoped(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase("resqueue");
        });

        services.AddScoped(sp =>
        {
            var database = sp.GetRequiredService<IMongoDatabase>();
            return database.GetCollection<Broker>("brokers");
        });

        services.AddScoped(sp =>
        {
            var database = sp.GetRequiredService<IMongoDatabase>();
            return database.GetCollection<Queue>("queues");
        });

        services.AddScoped(sp =>
        {
            var database = sp.GetRequiredService<IMongoDatabase>();
            return database.GetCollection<Exchange>("exchanges");
        });

        services.AddScoped(sp =>
        {
            var database = sp.GetRequiredService<IMongoDatabase>();
            return database.GetCollection<Message>("messages");
        });

        services.AddScoped(sp =>
        {
            var database = sp.GetRequiredService<IMongoDatabase>();
            return database.GetCollection<BrokerInvitation>("broker-invitations");
        });

        return services;
    }
}