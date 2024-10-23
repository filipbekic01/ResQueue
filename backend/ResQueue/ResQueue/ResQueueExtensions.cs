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

    public static IApplicationBuilder UseResQueue(this WebApplication app)
    {
        // string[] frontendRoutes =
        // [
        //     "^resqueue-ui"
        // ];
        //
        // app.UseRewriter(frontendRoutes.Aggregate(
        //     new RewriteOptions(),
        //     (options, route) => options.AddRewrite(route, "/_content/ResQueue.MassTransit/index.html", true))
        // );
        //
        // var apiGroup = app.MapGroup("resqueue-api");
        // apiGroup.MapQueueEndpoints();
        // apiGroup.MapMessageEndpoints();

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider =
                new PhysicalFileProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resqueue-wwwroot")),
            RequestPath = "/resqueue"
        });

        var apiGroup = app.MapGroup("resqueue-api");
        apiGroup.MapQueueEndpoints();
        apiGroup.MapMessageEndpoints();

        return app;
    }
}

// app.UseStaticFiles(new StaticFileOptions()
// {
//     FileProvider = embeddedProvider,
//     OnPrepareResponse = (context) =>
//     {
//         context.Context.Response.Headers.CacheControl =
//             context.Context.Request.Path.StartsWithSegments("/assets")
//                 ? "public, max-age=31536000, immutable"
//                 : "no-cache, no-store";
//     }
// });