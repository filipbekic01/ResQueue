using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Models;
using Resqueue.Models.MongoDB;

namespace Resqueue.Endpoints;

public static class MessageEndpoints
{
    public static IEndpointRouteBuilder MapMessageEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("messages")
            .RequireAuthorization();

        group.MapGet("{queueId}",
            async (IMongoCollection<Message> messagesCollection, UserManager<User> userManager, HttpContext httpContext,
                string queueId) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                if (!ObjectId.TryParse(queueId, out var queueObjectId))
                {
                    return Results.BadRequest("Invalid Broker ID format.");
                }

                var filter = Builders<Message>.Filter.And(
                    Builders<Message>.Filter.Eq(q => q.QueueId, queueObjectId)
                );

                var messages = await messagesCollection.Find(filter).ToListAsync();

                return Results.Ok(messages.Select(q => new MessageDto()
                {
                    Id = q.Id.ToString(),
                    RawData = q.RawData.ToString()
                }));
            });

        group.MapGet("{queueId}/sync",
            async (IHttpClientFactory httpClientFactory,
                IMongoCollection<Message> messagesCollection,
                IMongoCollection<Queue> queuesCollection,
                IMongoCollection<Broker> brokersCollection,
                UserManager<User> userManager,
                HttpContext httpContext,
                string queueId
            ) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var queueFilter = Builders<Queue>.Filter.Eq(b => b.Id, ObjectId.Parse(queueId));
                var queue = await queuesCollection.Find(queueFilter).FirstOrDefaultAsync();
                if (queue == null)
                {
                    return Results.Unauthorized();
                }

                var brokerFilter = Builders<Broker>.Filter.And(
                    Builders<Broker>.Filter.Eq(b => b.Id, ObjectId.Parse(queueId)),
                    Builders<Broker>.Filter.Eq(b => b.UserId, user.Id)
                );
                var broker = await brokersCollection.Find(brokerFilter).FirstOrDefaultAsync();
                if (broker == null)
                {
                    return Results.Unauthorized();
                }

                var http = httpClientFactory.CreateClient();
                http.BaseAddress = new Uri($"{broker.Url}:{broker.Port}");

                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", broker.Auth);
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Sync queues

                // vhost?
                var response = await http.GetAsync($"/api/queues/%2F/{queue.RawData.GetValue("name")}/get");
                response.EnsureSuccessStatusCode();

                var content1 = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(content1);
                var root = document.RootElement;

                return Results.Ok();
            });

        return group;
    }
}