# ResQueue

**ResQueue** (pronounced /ˈrɛskjuː/) is a web-based UI management tool designed for SQL-based message transports. Currently, it offers seamless integration with MassTransit, and we're open to adding support for additional frameworks based on user feedback and demand.

Join our community on [Discord](https://discord.gg/322AAB4xKx) for updates, support, and discussions.

All checked features are available in latest [NuGet](https://www.nuget.org/packages/ResQueue.MassTransit) version.

## Get Started

To set up **ResQueue** within your application, follow these simple steps:

```bash
dotnet add package ResQueue.MassTransit
```

```csharp
var builder = WebApplication.CreateBuilder(args);

// MassTransit configuration...
builder.Services.AddOptions<SqlTransportOptions>().Configure(options =>
{
    options.ConnectionString = "your_connection_string";
});

// migrations

// ResQueue uses SqlTransportOptions credentials internaly
builder.Services.AddResQueue(o => o.SqlEngine = ResQueueSqlEngine.Postgres);

// The order of migrations is very important
builder.Services.AddPostgresMigrationHostedService();
builder.Services.AddResQueueMigrationsHostedService();

var app = builder.Build();

app.UseResQueue("resqueue", options =>
{
    // Highly recommended for production
    options.RequireAuthorization();
});

app.Run();
```

> [!NOTE]
> You can create a standalone web application and deploy it. Simply add ResQueue nuget package and initialize it. It does not need to be tightly integrated with your existing application. There is also docker image below.

### Message Transformers

Transformers allow you to modify messages on the backend. They are used to generate useful data for the response, such as assembling links to your monitoring system or other relevant resources.

```csharp
builder.Services.AddResQueue(opt =>
{
    // Option 1: Custom transformer class
    opt.AddTransformer<CustomTransformer>();
    // ...see below for implementation

    // Option 2: Inline message transformer
    opt.AppendAdditionalData = (messageDeliveryDto) =>
    {
        return new()
        {
            { "CurrentDateTime", DateTime.UtcNow.ToString() },
            { "Custom", "Data" },
            { "SimpleLink", @"<a href='https://www.youtube.com/watch?v=dQw4w9WgXcQ' target='_blank'>This is a link</a>" }
        };
    };
});

// Option 1: The custom transformer class
public class CustomTransformer(
    IHttpContextAccessor httpContextAccessor
) : AbstractMessageTransformer
{
    public override Task<MessageDeliveryDto> TransformAsync(MessageDeliveryDto message)
    {
        message.AdditionalData ??= new();

        // Example using HTTP origin header
        message.AdditionalData["HttpOrigin"] = httpContextAccessor.HttpContext.Request.Headers["Origin"];

        return Task.FromResult(message);
    }
}
```

Additional data will appear in next format:

<img width="500" alt="image" src="https://github.com/user-attachments/assets/54e5c085-2d59-452c-8d06-d38208332a4f" />

### Docker support

Simplify your setup by running ResQueue in standalone mode with Docker. Get up and running effortlessly without additional configurations—just pull the container and you're ready to go.

```sh
docker run -it --rm -p 8080:8080 -e ResQueue:SqlEngine=Postgres -e SqlTransport:ConnectionString="Host=host.docker.internal;Database=DATABASE;Username=USERNAME;Password=PASSWORD;" ghcr.io/filipbekic01/resqueue
```

## UI Preview

Here's a quick preview of the ResQueue user interface, providing you with a glimpse of what to expect.

<img width="1840" alt="image" src="https://github.com/user-attachments/assets/b60acf98-68d3-40be-a400-cf21889bc458">
<img width="1617" alt="image" src="https://github.com/user-attachments/assets/167b0ff2-0dea-4cec-94cf-8dab24ac9d40">
<img width="1617" alt="image" src="https://github.com/user-attachments/assets/d7f894a2-7021-485f-9147-52694fa00524">
<img width="1840" alt="image" src="https://github.com/user-attachments/assets/0ed693cb-49d6-40d1-85db-94daed81dad6">
