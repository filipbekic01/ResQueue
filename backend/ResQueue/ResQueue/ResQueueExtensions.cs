using System.Reflection;
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

        if (builder.Environment.IsDevelopment())
        {
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
        }

        // todo: Remove if not used anymore.
        builder.Services.AddHttpClient();

        builder.Services.AddTransient<IRequeueMessagesFeature, RequeueMessagesFeature>();
        builder.Services.AddTransient<IRequeueSpecificMessagesFeature, RequeueSpecificMessagesFeature>();

        return builder;
    }

    public static IApplicationBuilder UseResQueue(this WebApplication app)
    {
        string[] frontendRoutes =
        {
            "^resqueue-ui",
        };

        app.UseRewriter(frontendRoutes.Aggregate(
            new RewriteOptions(),
            (options, route) => options.AddRewrite(route, "/index.html", true))
        );
        
        if (app.Environment.IsDevelopment())
        {
            app.UseCors("AllowAll");
            
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
        else
        {
            var assembly = typeof(ResQueueExtensions).GetTypeInfo().Assembly;
            var embeddedProvider = new EmbeddedFileProvider(assembly, "ResQueue.staticwebassets");
        
            app.UseDefaultFiles(new DefaultFilesOptions()
            {
                FileProvider = embeddedProvider
            });
        
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = embeddedProvider,
                OnPrepareResponse = (context) =>
                {
                    context.Context.Response.Headers.CacheControl =
                        context.Context.Request.Path.StartsWithSegments("/assets")
                            ? "public, max-age=31536000, immutable"
                            : "no-cache, no-store";
                }
            });
        }

        var apiGroup = app.MapGroup("resqueue-api");
        apiGroup.MapQueueEndpoints();
        apiGroup.MapMessageEndpoints();

        return app;
    }
}