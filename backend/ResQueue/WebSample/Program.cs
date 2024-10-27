using System.Reflection;
using Marten;
using MassTransit;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.FileProviders;
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

        builder.AddResQueue(opt =>
        {
            // postgres
            // opt.Host = "localhost";
            // opt.Database = "sandbox201";
            // opt.Schema = "transport";
            // opt.Username = "postgres";
            // opt.Password = "postgres";

            // sqlserver
            opt.Host = "localhost";
            opt.Database = "sandbox201";
            opt.Schema = "transport";
            opt.Username = "sa";
            opt.Password = "YourStrong!Passw0rd";
            opt.SqlEngine = ResQueueSqlEngine.SqlServer;
        });

        builder.Services.AddOptions<SqlTransportOptions>().Configure(options =>
        {
            // postgres
            // options.Host = "localhost";
            // options.Database = "sandbox201";
            // options.Schema = "transport";
            // options.Role = "transport";
            // options.Username = "postgres";
            // options.Password = "postgres";

            // sqlserver
            options.Host = "localhost";
            options.Database = "sandbox201";
            options.Schema = "transport";
            options.Role = "transport";
            options.Username = "sa";
            options.Password = "YourStrong!Passw0rd";
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Order is important because MassTransit will create database on boot, if missing.
        // builder.Services.AddPostgresMigrationHostedService();
        builder.Services.AddSqlServerMigrationHostedService();
        builder.Services.AddResQueueMigrationsHostedService();

        builder.Services.AddMarten(x =>
        {
            x.Connection("Host=localhost;Database=sandbox201;Username=postgres;Password=postgres;");
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

            // mt.UsingPostgres((context, config) =>
            // {
            //     config.UseSqlMessageScheduler();
            //     config.ConfigureEndpoints(context);
            // });

            mt.UsingSqlServer((context, config) =>
            {
                config.UseSqlMessageScheduler();
                config.ConfigureEndpoints(context);
            });
        });

        var app = builder.Build();
        app.UseCors("AllowAll");
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseResQueue("custom-prefix");

        app.MapGet("/publish",
            async (IPublishEndpoint endpoint) => { await endpoint.Publish(new YourMessage(Guid.NewGuid())); });

        app.Run();
    }
}