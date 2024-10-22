# ResQueue

**ResQueue** is a web-based UI management tool designed for SQL-based message transports. Currently, it offers seamless integration with MassTransit, and we're open to adding support for additional frameworks based on user feedback and demand.

Join our community on [Discord](https://discord.gg/322AAB4xKx) for updates, support, and discussions.

## Configuration

To set up **ResQueue**, follow these simple steps:

1. Install the latest version of `ResQueue.MassTransit` from NuGet to ensure compatibility with the official MassTransit updates:
    ```bash
    dotnet add package ResQueue.MassTransit
    ```

2. In your .NET application, configure **ResQueue** in the `WebApplication` builder by calling `builder.Services.AddResQueue()` with your database connection details. This can be done as follows:
    ```csharp
    var builder = WebApplication.CreateBuilder(args);

    // Add ResQueue with the necessary database configuration
    builder.Services.AddResQueue(options =>
    {
        // Configure database
    });

    var app = builder.Build();

    app.UseResQueue();

    // Run the app
    app.Run();
    ```

3. Once this is set up, your application should work right out of the box.

**ResQueue** will handle all the configuration and integration with MassTransit for you, making it simple to manage your SQL transports.
