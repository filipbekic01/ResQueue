using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Resqueue.Endpoints;
using Resqueue.Models;

namespace Resqueue;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<AppDbContext>(db => db.UseSqlite("DataSource=WebOneSql.db"));
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

        builder.Services.AddSingleton<IMongoClient, MongoClient>(sp =>
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://localhost:27018");
            return new MongoClient(settings);
        });

        builder.Services.ConfigureApplicationCookie(options => { options.ExpireTimeSpan = TimeSpan.FromDays(30); });
        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.UseCors("AllowAll");

        app.MapIdentityApi<User>();
        app.MapAuthEndpoints();
        app.MapBrokerEndpoints();

        app.MapPost("import", async (IMongoClient client, IHttpClientFactory httpClientFactory, string queueName) =>
        {
            var http = httpClientFactory.CreateClient();
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("rabbitmq:rabbitmq"));

            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response =
                await http.PostAsync(
                    $"https://localhost:15671/api/queues/%2F/{queueName}/get",
                    new StringContent(JsonSerializer.Serialize(new
                    {
                        count = 1111,
                        ackmode = "ack_requeue_true",
                        encoding = "auto",
                        truncate = 50000
                    }), Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            var content1 = await response.Content.ReadAsStringAsync();
            // var messages = JsonSerializer.Deserialize<List<RabbitMQMessage>>(content1)!.ToList();
            //
            // var db = client.GetDatabase("webone");
            // var collection = db.GetCollection<RabbitMQMessage>("rabbitmq");
            //
            // foreach (var message in messages)
            // {
            //     await collection.InsertOneAsync(message);
            // }
        });

        app.Run();
    }
}