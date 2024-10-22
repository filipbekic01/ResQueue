using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;
using ResQueue.Endpoints;
using ResQueue.Features.Messages.RequeueMessages;
using ResQueue.Features.Messages.RequeueSpecificMessages;

namespace ResQueue;

public static class ResQueueExtensions
{
    public static WebApplicationBuilder AddResQueue(this WebApplicationBuilder builder,
        Action<Settings> configureOptions)
    {
        // Configure IOptions<ResQueueOptions>
        builder.Services.Configure(configureOptions);

        // Configure CORS
        builder.Services.AddCors(corsOptions =>
        {
            corsOptions.AddPolicy("AllowAll", policy =>
            {
                policy.SetIsOriginAllowed(_ => true);
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.AllowCredentials();
            });
        });

        // Add HTTP Client
        builder.Services.AddHttpClient();

        // Register features
        builder.Services.AddTransient<IRequeueMessagesFeature, RequeueMessagesFeature>();
        builder.Services.AddTransient<IRequeueSpecificMessagesFeature, RequeueSpecificMessagesFeature>();

        return builder;
    }

    public static IApplicationBuilder UseResQueue(this WebApplication app)
    {
        // Use CORS policy
        app.UseCors("AllowAll");

        // Temporary internal server error fix
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler(configure => configure.Run(_ => Task.CompletedTask));
        }

        // Configure frontend route rewriting
        string[] frontendRoutes =
        {
            "^login",
            "^forgot-password$",
            "^support$",
            "^pricing$",
            "^app",
        };

        app.UseRewriter(frontendRoutes.Aggregate(
            new RewriteOptions(),
            (options, route) => options.AddRewrite(route, "/index.html", true))
        );

        // Serve static files
        app.UseDefaultFiles();
        app.UseStaticFiles(new StaticFileOptions()
        {
            OnPrepareResponse = (context) =>
            {
                context.Context.Response.Headers.CacheControl =
                    context.Context.Request.Path.StartsWithSegments("/assets")
                        ? "public, max-age=31536000, immutable"
                        : "no-cache, no-store";
            }
        });

        // Map API endpoints
        var apiGroup = app.MapGroup("resqueue");
        apiGroup.MapQueueEndpoints();
        apiGroup.MapMessageEndpoints();

        return app;
    }
}