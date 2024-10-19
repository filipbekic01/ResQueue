using Microsoft.AspNetCore.Rewrite;
using ResQueue.Endpoints;
using ResQueue.Features.Messages.MoveMessage;
using ResQueue.Features.Messages.RequeueMessages;

namespace ResQueue;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var settingsSection = builder.Configuration.GetRequiredSection("Settings");
        builder.Services.Configure<Settings>(settingsSection);
        var settings = settingsSection.Get<Settings>()!;

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

        builder.Services.AddHttpClient();

        builder.Services.AddTransient<IRequeueMessagesFeature, RequeueMessagesFeature>();
        builder.Services.AddTransient<IRequeueSpecificMessagesFeature, RequeueSpecificMessagesFeature>();

        var app = builder.Build();

        app.UseCors("AllowAll");

        // Temporary internal server error fix until this issue is officially fixed.
        // https://github.com/dotnet/aspnetcore/issues/22281
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler(configure => configure.Run(_ => Task.CompletedTask));
        }

        string[] frontendRoutes =
        [
            "^login",
            "^forgot-password$",
            "^support$",
            "^pricing$",
            "^app",
        ];
        app.UseRewriter(frontendRoutes.Aggregate(
            new RewriteOptions(),
            (options, route) => options.AddRewrite(route, "/index.html", true))
        );
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

        var apiGroup = app.MapGroup("api");
        apiGroup.MapQueueEndpoints();
        apiGroup.MapMessageEndpoints();

        app.Run();
    }
}