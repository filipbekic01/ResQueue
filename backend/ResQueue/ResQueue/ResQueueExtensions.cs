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

        var rewriteOptions = new RewriteOptions()
            .AddRewrite("^(.*)/$", "$1", false); // Remove trailing slash

        string[] frontendRoutes =
        [
            "",
            "/overview",
            "/topics",
            "/queues",
            "/jobs",
            "/queues/[^/]+",
        ];

        rewriteOptions = frontendRoutes.Aggregate(rewriteOptions, (options, route) => options.AddRewrite(
            regex: $"^{prefix}{route}$",
            replacement: "/resqueue-4e8efb80-6aae-496f-b8bf-611b63e725bc/index.html",
            skipRemainingRules: true
        ));

        app.UseRewriter(rewriteOptions);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider =
                new PhysicalFileProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resqueue-wwwroot")),
            RequestPath = "/resqueue-4e8efb80-6aae-496f-b8bf-611b63e725bc"
        });

        var api = app.MapGroup("resqueue-4e8efb80-6aae-496f-b8bf-611b63e725bc/api");
        configureApi?.Invoke(api);
        api.MapAuthEndpoints();
        api.MapQueueEndpoints();
        api.MapMessageEndpoints();

        return app;
    }
}