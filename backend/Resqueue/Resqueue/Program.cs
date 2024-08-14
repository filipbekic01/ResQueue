using Microsoft.AspNetCore.Identity;
using Resqueue.Endpoints;
using Resqueue.Models;

namespace Resqueue;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCors(corsOptions =>
        {
            corsOptions.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins("http://localhost:5173");
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.AllowCredentials();
            });
        });

        builder.Services.AddHttpClient();

        builder.Services.AddMongoDb();

        builder.Services.AddSingleton<IEmailSender<User>, DummyEmailSender>();

        builder.Services.ConfigureApplicationCookie(options => { options.ExpireTimeSpan = TimeSpan.FromDays(30); });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.UseCors("AllowAll");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapIdentityApi<User>();
        app.MapAuthEndpoints();
        app.MapBrokerEndpoints();
        app.MapQueueEndpoints();
        app.MapExchangeEndpoints();
        app.MapMessageEndpoints();

        app.Run();
    }
}