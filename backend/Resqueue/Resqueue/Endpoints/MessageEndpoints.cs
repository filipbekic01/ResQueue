using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        // ack from queue
        group.MapPost("{queueId}/sync",
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
                    Builders<Broker>.Filter.Eq(b => b.Id, queue.BrokerId),
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

                var requestBody = new
                {
                    count = 10, // Number of messages to fetch
                    ackmode = "ack_requeue_true", // Acknowledge and requeue the messages
                    encoding = "auto",
                    truncate = 50000
                };

                // vhost?
                var response =
                    await http.PostAsync($"/api/queues/%2F/{queue.RawData.GetValue("name")}/get",
                        new StringContent(JsonSerializer.Serialize(requestBody)));
                response.EnsureSuccessStatusCode();

                var content1 = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(content1);
                var root = document.RootElement;

                var messages = new List<Message>();
                foreach (var element in root.EnumerateArray())
                {
                    messages.Add(new Message()
                    {
                        QueueId = queue.Id,
                        RawData = BsonDocument.Parse(element.GetRawText()),
                        Summary = "Contextual summary of messages",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (messages.Count > 0)
                {
                    await messagesCollection.InsertManyAsync(messages);
                }

                return Results.Ok();
            });

        group.MapDelete("{queueId}", async (string[] messageIds) =>
        {
            // todo: archive messages
        });

        group.MapPost("publish",
            async (IHttpClientFactory httpClientFactory,
                IMongoCollection<Message> messagesCollection,
                IMongoCollection<Exchange> exchangesCollection,
                IMongoCollection<Broker> brokersCollection,
                UserManager<User> userManager,
                HttpContext httpContext,
                [FromBody] PublishDto dto
            ) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var exchange = await exchangesCollection
                    .Find(Builders<Exchange>.Filter.Eq(b => b.Id, ObjectId.Parse(dto.ExchangeId)))
                    .FirstOrDefaultAsync();
                if (exchange == null)
                {
                    return Results.Unauthorized();
                }

                var broker = await brokersCollection.Find(Builders<Broker>.Filter.And(
                    Builders<Broker>.Filter.Eq(b => b.Id, exchange.BrokerId),
                    Builders<Broker>.Filter.Eq(b => b.UserId, user.Id)
                )).FirstOrDefaultAsync();
                if (broker == null)
                {
                    return Results.Unauthorized();
                }

                var messagesFilter =
                    Builders<Message>.Filter.In(b => b.Id, dto.MessageIds.Select(ObjectId.Parse).ToList());
                var messages = await messagesCollection.Find(messagesFilter).ToListAsync();

                var http = httpClientFactory.CreateClient();
                http.BaseAddress = new Uri($"{broker.Url}:{broker.Port}");

                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", broker.Auth);
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Please note that the HTTP API is not ideal for high performance publishing; the need to create a new
                // TCP connection for each message published can limit message throughput compared to AMQP or other
                // protocols using long-lived connections.

                foreach (var message in messages)
                {
                    var requestBody = new
                    {
                        properties = new { },
                        routing_key = exchange.RawData.GetValue("name").ToString(),
                        payload = message.RawData.GetValue("payload").ToString(),
                        payload_encoding = message.RawData.GetValue("payload_encoding").ToString()
                    };

                    // vhost?
                    var response =
                        await http.PostAsync($"/api/exchanges/%2F/{exchange.RawData.GetValue("name")}/publish",
                            new StringContent(JsonSerializer.Serialize(requestBody)));

                    response.EnsureSuccessStatusCode();
                }

                await messagesCollection.UpdateOneAsync(
                    Builders<Message>.Filter
                        .In(b => b.Id, messages.Select(x => x.Id).ToList()),
                    Builders<Message>.Update
                        .Set(b => b.DeletedAt, DateTime.UtcNow));

                return Results.Ok();
            });

        return group;
    }
}