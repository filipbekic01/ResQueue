using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.FileProviders;
using ResQueue.Endpoints;
using ResQueue.Features.Messages.RequeueMessages;
using ResQueue.Features.Messages.RequeueSpecificMessages;

namespace ResQueue;

public static class ResQueueExtensions
{
    public static WebApplicationBuilder AddResQueue(this WebApplicationBuilder builder,
        Action<Settings> configureOptions)
    {
        builder.Services.Configure(configureOptions);

        // todo: Remove if not used anymore.
        builder.Services.AddHttpClient();

        builder.Services.AddTransient<IRequeueMessagesFeature, RequeueMessagesFeature>();
        builder.Services.AddTransient<IRequeueSpecificMessagesFeature, RequeueSpecificMessagesFeature>();

        return builder;
    }

    public static IApplicationBuilder UseResQueue(this WebApplication app, string prefix = "resqueue",
        Action<RouteGroupBuilder>? configureApi = null)
    {
        app.MapGet("resqueue-4e8efb80-6aae-496f-b8bf-611b63e725bc/config.js", () => Results.Content(
            $$"""
              globalThis.resqueueConfig = {
                  prefix: "{{prefix}}"
              }
              """
            , "text/javascript"
        ));

        string[] frontendRoutes =
        [
            "",
        ];

        app.UseRewriter(frontendRoutes.Aggregate(
            new RewriteOptions(),
            (options, route) => options.AddRewrite($"^{prefix}{route}$",
                "/resqueue-4e8efb80-6aae-496f-b8bf-611b63e725bc/index.html", true))
        );

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider =
                new PhysicalFileProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resqueue-wwwroot")),
            RequestPath = "/resqueue-4e8efb80-6aae-496f-b8bf-611b63e725bc"
        });

        var apiGroup = app.MapGroup("resqueue-4e8efb80-6aae-496f-b8bf-611b63e725bc/api");
        configureApi?.Invoke(apiGroup);
        apiGroup.MapQueueEndpoints();
        apiGroup.MapMessageEndpoints();

        return app;
    }
}