# ResQueue

**ResQueue** (pronounced /ˈrɛskjuː/) is a web-based UI management tool designed for SQL-based message transports. Currently, it offers seamless integration with MassTransit, and we're open to adding support for additional frameworks based on user feedback and demand.

Join our community on [Discord](https://discord.gg/322AAB4xKx) for updates, support, and discussions.

### Work in Progress

All checked features are available in latest [NuGet](https://www.nuget.org/packages/ResQueue.MassTransit) version.

#### Brokers

- [x] MassTransit
  - [x] MassTransit.SqlTransport.PostgreSQL
  - [x] MassTransit.SqlTransport.SqlServer

#### Features

- [x] Tabular broker information view
- [x] Tabular queues view
- [x] Tabluar messages view
  - [x] Requeue selected messages (transactional and non-transactional)
  - [x] Requeue first N messages from the top of the queue
  - [x] Delete selected messages (transactional and non-transactional)
  - [x] Purge all messages
  - [x] Single message view
    - [x] Job state preview if present
- [ ] Tabuar topics view
- [ ] Tabular recurring jobs view

## Configuration

To set up **ResQueue**, follow these simple steps:

1. Install the latest version of `ResQueue.MassTransit` from NuGet to ensure compatibility with the official MassTransit updates:

```bash
dotnet add package ResQueue.MassTransit
```

2. In your .NET application, configure **ResQueue** in the `WebApplication` builder by calling `builder.Services.AddResQueue()` with your database connection details. This can be done as follows:

> [!WARNING]
> ResQueue configuration must follow the MassTransit setup, as MassTransit is a prerequisite for ResQueue to function correctly. Required MassTransit version is at least 8.3.1-develop.2100.

```csharp
var builder = WebApplication.CreateBuilder(args);

// MassTransit configuration...

// ResQueue relays on MassTransit.SqlTransportOptions credentials
builder.Services.AddResQueue(o => o.SqlEngine = ResQueueSqlEngine.Postgres);

// Make sure you add this line after MassTransit SQL migrations hosted service
builder.Services.AddResQueueMigrationsHostedService();

var app = builder.Build();

app.UseResQueue("resqueue", options =>
{
    // Recommended for production environments, add roles too
    options.RequireAuthorization();
});

app.Run();
```

3. Once this is set up, your application should work right out of the box.

**ResQueue** will handle all the configuration and integration with MassTransit for you, making it simple to manage your SQL transports.

## UI Preview

Here's a quick preview of the ResQueue user interface, providing you with a glimpse of what to expect.

<img width="1840" alt="image" src="https://github.com/user-attachments/assets/b60acf98-68d3-40be-a400-cf21889bc458">
<img width="1617" alt="image" src="https://github.com/user-attachments/assets/167b0ff2-0dea-4cec-94cf-8dab24ac9d40">
<img width="1617" alt="image" src="https://github.com/user-attachments/assets/d7f894a2-7021-485f-9147-52694fa00524">
<img width="1840" alt="image" src="https://github.com/user-attachments/assets/0ed693cb-49d6-40d1-85db-94daed81dad6">

