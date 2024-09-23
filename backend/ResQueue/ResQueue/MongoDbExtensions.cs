using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ResQueue.HostedServices;
using ResQueue.Models;

namespace ResQueue;

public static class MongoDbExtensions
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services)
    {
        services.AddSingleton<IMongoClient, MongoClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<Settings>>();
            return new MongoClient(MongoClientSettings.FromConnectionString(options.Value.MongoDBConnectionString));
        });

        var mongoClient = services.BuildServiceProvider().GetRequiredService<IMongoClient>();
        var mongoDbXmlRepository = new MongoDbXmlRepository(mongoClient, "resqueue");

        services.AddDataProtection();
        services.Configure<KeyManagementOptions>(o => { o.XmlRepository = mongoDbXmlRepository; });

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

        services.AddHostedService<MongoDbIndexManagerService>();

        return services;
    }
}