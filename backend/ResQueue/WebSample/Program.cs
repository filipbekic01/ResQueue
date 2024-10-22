using Marten;
using MassTransit;
using ResQueue;

namespace WebSample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddResQueue(opt =>
        {
            opt.PostgreSQLConnectionString =
                "Host=localhost;Database=sandbox100;Username=postgres;Password=postgres;";
        });

        // Add services to the container.
        builder.Services.AddOptions<SqlTransportOptions>().Configure(options =>
        {
            options.Host = "localhost";
            options.Database = "sandbox100";
            options.Schema = "transport";
            options.Role = "transport";
            options.Username = "postgres";
            options.Password = "postgres";
        });

        builder.Services.AddPostgresMigrationHostedService();

        builder.Services.AddMarten(x =>
        {
            x.Connection("Host=localhost;Database=sandbox100;Username=postgres;Password=postgres;");
        });

        builder.Services.AddMassTransit(mt =>
        {
            mt.AddSqlMessageScheduler();

            mt.SetMartenSagaRepositoryProvider();

            mt.AddConsumer<YourConsumer>()
                .Endpoint(e => { e.ConcurrentMessageLimit = 1; });

            mt.AddConsumer<AwesomeConsumer>()
                .Endpoint(e => { e.ConcurrentMessageLimit = 1; });

            mt.AddJobSagaStateMachines();

            mt.UsingPostgres((context, config) =>
            {
                config.UseSqlMessageScheduler();
                config.ConfigureEndpoints(context);
            });
        });

        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        app.UseResQueue();

        app.Run();
    }
}