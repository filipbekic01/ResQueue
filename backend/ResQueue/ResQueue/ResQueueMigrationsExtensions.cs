using ResQueue.Migrations;

namespace ResQueue;

public static class ResQueueMigrationsExtensions
{
    public static IServiceCollection AddResQueueMigrationsHostedService(this IServiceCollection services)
    {
        services.AddHostedService<SqlMigrations>();

        return services;
    }
}