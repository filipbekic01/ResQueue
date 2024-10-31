using Marten;
using MassTransit;
using MassTransit.Contracts.JobService;
using Microsoft.AspNetCore.Http.HttpResults;
using ResQueue;
using ResQueue.Enums;

namespace WebSample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCors(corsOptions =>
        {
            corsOptions.AddPolicy("AllowAll", policy =>
            {
                policy.SetIsOriginAllowed(_ => true);
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.AllowCredentials();
            });
        });

        // Server=localhost,1433;Database=sandbox203;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True
        // Host=localhost;Database=sandbox203;Username=postgres;Password=postgres;
        builder.Services.AddResQueue(opt =>
        {
            opt.SqlEngine = ResQueueSqlEngine.Postgres;
            opt.ConnectionString =
                "Host=localhost;Database=sandbox203;Username=postgres;Password=postgres;";
        });
        builder.Services.AddOptions<SqlTransportOptions>().Configure(options =>
        {
            options.ConnectionString =
                "Host=localhost;Database=sandbox203;Username=postgres;Password=postgres;";
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddPostgresMigrationHostedService();
        // builder.Services.AddSqlServerMigrationHostedService();

        // Must go after MassTransit migrations
        builder.Services.AddResQueueMigrationsHostedService();

        builder.Services.AddMarten(x =>
        {
            x.Connection("Host=localhost;Database=sandbox203;Username=postgres;Password=postgres;");
        });

        builder.Services.AddMassTransit(mt =>
        {
            mt.AddSqlMessageScheduler();

            mt.SetMartenSagaRepositoryProvider();

            mt.AddConsumer<YourConsumer>()
                .Endpoint(e => { e.ConcurrentMessageLimit = 1; });

            mt.AddConsumer<YourConsumer1>()
                .Endpoint(e => { e.ConcurrentMessageLimit = 1; });

            mt.AddConsumer<AwesomeConsumer>()
                .Endpoint(e => { e.ConcurrentMessageLimit = 1; });

            mt.AddJobSagaStateMachines();

            mt.UsingPostgres((context, config) =>
            {
                config.UseSqlMessageScheduler();
                config.ConfigureEndpoints(context);
            });

            // mt.UsingSqlServer((context, config) =>
            // {
            //     config.UseSqlMessageScheduler();
            //     config.ConfigureEndpoints(context);
            // });
        });

        var app = builder.Build();
        app.UseCors("AllowAll");
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseResQueue();

        app.MapGet("/publish",
            async (IPublishEndpoint endpoint) => { await endpoint.Publish(new YourMessage(Guid.NewGuid())); });

        app.MapGet("/schedule",
            async (IPublishEndpoint endpoint) =>
            {
                var jobId = await endpoint.AddOrUpdateRecurringJob(nameof(AwesomeConsumer), new AwesomeRequest(),
                    x => x.Every(minutes: 1));

                return Results.Ok(jobId);
            });

        app.MapGet("/cancel/{jobId:guid}",
            async (IPublishEndpoint endpoint, Guid jobId) =>
            {
                await endpoint.CancelJob(jobId, "hoobastank");

                return Results.Ok();
            });

        app.MapGet("/state/{jobId:guid}",
            async (IRequestClient<GetJobState> client, Guid jobId) =>
            {
                var state = await client.GetJobState(jobId);

                return Results.Ok(state);
            });

        app.Run();
    }
}