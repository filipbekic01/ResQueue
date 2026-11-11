using MassTransit;
using ResQueue;
using ResQueue.Enums;

var builder = WebApplication.CreateBuilder(args);

var sqlEngineConfig = builder.Configuration.GetRequiredSection("Resqueue:SqlEngine").Value;
if (string.IsNullOrEmpty(sqlEngineConfig))
{
    throw new Exception("Missing SqlEngine configuration in Resqueue:SqlEngine.");
}

var sqlEngine = Enum.Parse<ResQueueSqlEngine>(sqlEngineConfig);

builder.Services.AddResQueue(opt =>
{
    opt.SqlEngine = sqlEngine;

    // e.g. -e ResQueue__QueueDepthHistory__Enabled=true or ResQueue__QueueDepthHistory__Retention=3.00:00:00
    builder.Configuration.GetSection("Resqueue:QueueDepthHistory").Bind(opt.QueueDepthHistory);
});

builder.Services.AddOptions<SqlTransportOptions>().Configure(options =>
{
    builder.Configuration.Bind("SqlTransport", options);
});

if (sqlEngine == ResQueueSqlEngine.Postgres)
{
    builder.Services.AddPostgresMigrationHostedService();
}
else
{
    builder.Services.AddSqlServerMigrationHostedService();
}

builder.Services.AddResQueueMigrationsHostedService();

builder.Services.AddMassTransit(mt =>
{
    if (sqlEngine == ResQueueSqlEngine.Postgres)
    {
        mt.UsingPostgres();
    }
    else
    {
        mt.UsingSqlServer();
    }
});

var app = builder.Build();

app.UseResQueue("");

app.Run();