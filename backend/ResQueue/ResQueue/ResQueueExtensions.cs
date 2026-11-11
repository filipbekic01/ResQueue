using System.Text.Json;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.FileProviders;
using ResQueue.Endpoints;
using ResQueue.Factories;
using ResQueue.Features.Messages.DeleteMessages;
using ResQueue.Features.Messages.GetMessages;
using ResQueue.Features.Messages.GetSingleMessage;
using ResQueue.Features.Messages.PurgeQueue;
using ResQueue.Features.Messages.RequeueMessages;
using ResQueue.Features.Messages.RequeueSpecificMessages;
using ResQueue.Features.Subscriptions.GetSubscriptions;
using ResQueue.Middleware;
using ResQueue.Providers.DbConnectionProvider;

namespace ResQueue;

public static class ResQueueExtensions
{
    public static IServiceCollection AddResQueue(this IServiceCollection services,
        Action<ResQueueOptions> configureOptions)
    {
        services.Configure(configureOptions);

        // register the transformer types declared in the options
        var options = new ResQueueOptions();
        configureOptions(options);
        foreach (var transformerType in options.TransformerTypes)
        {
            services.AddScoped(transformerType);
        }

        services.AddSingleton<IDatabaseConnectionFactory, DatabaseConnectionFactory>();
        services.AddSingleton<IDbConnectionProvider, DbConnectionProvider>();

        services.AddTransient<IRequeueMessagesFeature, RequeueMessagesFeature>();
        services.AddTransient<IRequeueSpecificMessagesFeature, RequeueSpecificMessagesFeature>();
        services.AddTransient<IDeleteMessagesFeature, DeleteMessagesFeature>();
        services.AddTransient<IGetMessagesFeature, GetMessagesFeature>();
        services.AddTransient<IGetSingleMessageFeature, GetSingleMessageFeature>();
        services.AddTransient<IPurgeQueueFeature, PurgeQueueFeature>();
        services.AddTransient<IGetSubscriptionsFeature, GetSubscriptionsFeature>();

        return services;
    }

    public static IApplicationBuilder UseResQueue(this WebApplication app, string prefix = "resqueue",
        Action<RouteGroupBuilder>? configureApi = null)
    {
        app.UseMiddleware<ResQueueExceptionMiddleware>();

        // Served by middleware rather than as an endpoint so it short-circuits like the static files do.
        // Endpoints run at the end of the pipeline, so app middleware registered after UseResQueue
        // (auth, tenant resolution, redirects...) would otherwise intercept it.
        var configJs = $$"""
                         globalThis.resqueueConfig = {
                             prefix: {{JsonSerializer.Serialize(prefix)}}
                         }
                         """;

        ((IApplicationBuilder)app).Map("/resqueue-4e8efb80-6aae-496f-b8bf-611b63e725bc/config.js", configApp =>
            configApp.Run(context =>
            {
                context.Response.ContentType = "text/javascript";
                return context.Response.WriteAsync(configJs);
            }));

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

        var api = app.MapGroup("resqueue-4e8efb80-6aae-496f-b8bf-611b63e725bc/api")
            .ExcludeFromDescription();

        configureApi?.Invoke(api);
        api.MapAuthEndpoints();
        api.MapQueueEndpoints();
        api.MapMessageEndpoints();
        api.MapJobsEndpoints();
        api.MapSubscriptionsEndpoints();

        return app;
    }
}