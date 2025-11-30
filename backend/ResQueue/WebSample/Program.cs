using Marten;
using MassTransit;
using ResQueue;
using ResQueue.Enums;
using WebSample.Consumers;

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

        builder.Services.AddResQueue(opt =>
        {
            // opt.SqlEngine = ResQueueSqlEngine.SqlServer;
            opt.SqlEngine = ResQueueSqlEngine.Postgres;

            opt.AppendAdditionalData = msg =>
            {
                msg.AdditionalData.Add("Example-Data", "Example header value");

                return msg.AdditionalData;
            };
        });

        builder.Services.AddOptions<SqlTransportOptions>().Configure(options =>
        {
            // options.ConnectionString = builder.Configuration["SQL"] ?? throw new NullReferenceException();
            options.ConnectionString = builder.Configuration["Postgres"] ?? throw new NullReferenceException();
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddPostgresMigrationHostedService();
        // builder.Services.AddSqlServerMigrationHostedService();

        // Must go after MassTransit migrations
        builder.Services.AddResQueueMigrationsHostedService();

        builder.Services.AddMarten(x =>
        {
            x.Connection(builder.Configuration["Postgres"] ?? throw new NullReferenceException());
        });

        builder.Services.AddMassTransit(mt =>
        {
            mt.AddSqlMessageScheduler();

            mt.SetMartenSagaRepositoryProvider();

            // 1. Schedule party consumer (for scheduled messages in 3 days)
            mt.AddConsumer<SchedulePartyConsumer>()
                .Endpoint(e => { e.ConcurrentMessageLimit = 1; });

            // 2. Birthday invite consumer (always fails, for testing requeue with 8h TTL)
            mt.AddConsumer<BirthdayInviteConsumer>()
                .Endpoint(e => { e.ConcurrentMessageLimit = 1; });

            // 3. Drink order consumer (fails for under 18, retries until dead-letter)
            mt.AddConsumer<DrinkOrderConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r => r.Immediate(3));
            }).Endpoint(e => { e.ConcurrentMessageLimit = 1; });

            // 4. Weather check job consumer (recurring every 3 minutes)
            mt.AddConsumer<WeatherCheckConsumer>();

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
        app.MapTestEndpoints();

        app.Run();
    }
}