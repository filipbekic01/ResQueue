using AspNetCore.Identity.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Models;

namespace Resqueue;

public static class MongoDbExtensions
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services)
    {
        const string mongoOptionsConnectionString = "mongodb://localhost:27018";

        services.AddIdentityMongoDbProvider<User, Role, ObjectId>(opt =>
            {
                opt.Password.RequiredLength = 6;
                opt.Password.RequireDigit = false;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
            },
            mongoOptions => { mongoOptions.ConnectionString = $"{mongoOptionsConnectionString}/identity"; });

        services.AddSingleton<IMongoClient, MongoClient>(_ =>
        {
            var settings = MongoClientSettings.FromConnectionString(mongoOptionsConnectionString);
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

        return services;
    }
}