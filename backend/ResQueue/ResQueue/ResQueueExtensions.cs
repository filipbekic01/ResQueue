using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using ResQueue.Endpoints;
using ResQueue.Enums;
using ResQueue.Factories;
using ResQueue.Features.Messages.DeleteMessages;
using ResQueue.Features.Messages.GetMessages;
using ResQueue.Features.Messages.PurgeQueue;
using ResQueue.Features.Messages.RequeueMessages;
using ResQueue.Features.Messages.RequeueSpecificMessages;
using ResQueue.Migrations;
using ResQueue.Providers.DbConnectionProvider;

namespace ResQueue;

public static class ResQueueExtensions
{
    public static IServiceCollection AddResQueue(this IServiceCollection services,
        Action<ResQueueOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddSingleton<IDatabaseConnectionFactory, DatabaseConnectionFactory>();
        services.AddSingleton<IDbConnectionProvider, DbConnectionProvider>();

        services.AddTransient<IRequeueMessagesFeature, RequeueMessagesFeature>();
        services.AddTransient<IRequeueSpecificMessagesFeature, RequeueSpecificMessagesFeature>();
        services.AddTransient<IDeleteMessagesFeature, DeleteMessagesFeature>();
        services.AddTransient<IGetMessagesFeature, GetMessagesFeature>();
        services.AddTransient<IPurgeQueueFeature, PurgeQueueFeature>();

        return services;
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
            "/recurring-jobs",
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
        api.MapJobsEndpoints();

        return app;
    }
}