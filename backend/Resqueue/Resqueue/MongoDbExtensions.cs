using AspNetCore.Identity.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Models;

namespace Resqueue;

public static class MongoDbExtensions
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, Settings settings)
    {
        services.AddIdentityMongoDbProvider<User, Role, ObjectId>(opt =>
            {
                opt.Password.RequiredLength = 4;
                opt.Password.RequireDigit = false;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
                
                opt.User.RequireUniqueEmail = true;
            },
            mongoOptions => { mongoOptions.ConnectionString = $"{settings.MongoDBConnectionString}/identity"; });

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

        return services;
    }
}