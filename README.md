# ResQueue

> [!NOTE]
This project is still under active development. A beta version is currently available, but we cannot guarantee that it will function as expected. The official release is anticipated in November, 2024.

**ResQueue** (pronounced /ˈrɛskjuː/) is a web-based UI management tool designed for SQL-based message transports. Currently, it offers seamless integration with MassTransit, and we're open to adding support for additional frameworks based on user feedback and demand.

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
        // Credentials for MassTransit SQL database
    });

    var app = builder.Build();

    // Basic setup will open the ResQueue UI at the default path, /resqueue, in
    // the browser. By default, no authorization is applied, just clean endpoints, 
    // usually good enough for development.

    // app.UseResQueue();

    // Production code should use a custom URL prefix for the ResQueue UI. In this
    // case, the UI will be accessible at /custom-prefix. You can also apply
    // additional configuration, such as adding authorization or other middleware.
    app.UseResQueue("custom-prefix", options =>
    {
        options.RequireAuthorization();
    });

    // Run the app
    app.Run();
    ```

3. Once this is set up, your application should work right out of the box.

**ResQueue** will handle all the configuration and integration with MassTransit for you, making it simple to manage your SQL transports.

## UI Preview

Here's a quick preview of the ResQueue user interface, providing you with a glimpse of what to expect.

<img width="1840" alt="image" src="https://github.com/user-attachments/assets/b60acf98-68d3-40be-a400-cf21889bc458">
<img width="1840" alt="image" src="https://github.com/user-attachments/assets/41256c63-40c1-4017-bdf2-41947333718e">
<img width="1840" alt="image" src="https://github.com/user-attachments/assets/91a1fe90-83a0-468d-af7b-cf60b69f8feb">
<img width="1840" alt="image" src="https://github.com/user-attachments/assets/861070ed-f08f-49b4-bcc6-20f5b60efd30">

