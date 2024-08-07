using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Resqueue.Endpoints;
using Resqueue.Models;
using Resqueue.Models.MongoDB;

namespace Resqueue;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<AppDbContext>(db => db.UseSqlite("DataSource=local.db"));
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

        builder.Services.AddIdentityApiEndpoints<User>()
            .AddEntityFrameworkStores<AppDbContext>();

        builder.Services.AddMongoDb();

        builder.Services.ConfigureApplicationCookie(options => { options.ExpireTimeSpan = TimeSpan.FromDays(30); });
        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.UseCors("AllowAll");

        app.MapIdentityApi<User>();
        app.MapAuthEndpoints();
        app.MapBrokerEndpoints();
        app.MapQueueEndpoints();
        app.MapExchangeEndpoints();
        app.MapMessageEndpoints();

        app.Run();
    }
}