using MongoDB.Driver;
using Resqueue.Models.MongoDB;

namespace Resqueue;

public static class MongoDbExtensions
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services)
    {
        services.AddSingleton<IMongoClient, MongoClient>(sp =>
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://localhost:27018");
            return new MongoClient(settings);
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

        return services;
    }
}