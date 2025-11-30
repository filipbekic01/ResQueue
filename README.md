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

![New Project](https://github.com/user-attachments/assets/b63cc78b-2a24-436f-a4e1-5d72d020ee97)

### Docker support

Simplify your setup by running ResQueue in standalone mode with Docker. Get up and running effortlessly without additional configurations—just pull the container and you're ready to go.

```sh
docker run -it --rm -p 8080:8080 -e ResQueue:SqlEngine=Postgres -e SqlTransport:ConnectionString="Host=host.docker.internal;Database=DATABASE;Username=USERNAME;Password=PASSWORD;" ghcr.io/filipbekic01/resqueue
```

## UI Preview

Here's a quick preview of the ResQueue user interface, providing you with a glimpse of what to expect.

![image psd](https://github.com/user-attachments/assets/43392d7f-deb8-4ed1-a4e7-92ed6bd26ab1)
![image psd (1)](https://github.com/user-attachments/assets/9e97b996-aa1b-4abe-a6a5-5f1c82cc5671)
![image psd (5)](https://github.com/user-attachments/assets/118eab83-a4c0-4b62-ae1c-2564dde5899c)
![image psd (4)](https://github.com/user-attachments/assets/79e10bb9-5448-4cbe-8a23-8c0b21ab5ff0)
![image psd (3)](https://github.com/user-attachments/assets/e5e77db6-1383-4b39-a367-9d9b279f627b)
![image psd (2)](https://github.com/user-attachments/assets/c4189a86-1163-489f-854a-019354254da4)

