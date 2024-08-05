using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Models;
using Resqueue.Models.MongoDB;

namespace Resqueue.Endpoints;

public static class QueueEndpoints
{
    public static IEndpointRouteBuilder MapQueueEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("queues")
            .RequireAuthorization();

        group.MapGet("{brokerId}",
            async (IMongoCollection<Queue> collection, UserManager<User> userManager, HttpContext httpContext,
                string brokerId) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                if (!ObjectId.TryParse(brokerId, out var brokerObjectId))
                {
                    return Results.BadRequest("Invalid Broker ID format.");
                }

                var filter = Builders<Queue>.Filter.And(
                    Builders<Queue>.Filter.Eq(q => q.UserId, user.Id),
                    Builders<Queue>.Filter.Eq(q => q.BrokerId, brokerObjectId)
                );

                var queues = await collection.Find(filter).ToListAsync();

                return Results.Ok(queues.Select(q => new QueueDto()
                {
                    Id = q.Id.ToString(),
                    Name = q.Name,
                    Data = q.Data
                }));
            });

        // group.MapPost("sync", async (IMongoClient client, IHttpClientFactory httpClientFactory, string queueName) =>
        // {
        //     var http = httpClientFactory.CreateClient();
        //     var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("rabbitmq:rabbitmq"));
        //
        //     http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        //     http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //
        //     var response =
        //         await http.PostAsync(
        //             $"https://localhost:15671/api/queues/%2F/{queueName}/get",
        //             new StringContent(JsonSerializer.Serialize(new
        //             {
        //                 count = 1111,
        //                 ackmode = "ack_requeue_true",
        //                 encoding = "auto",
        //                 truncate = 50000
        //             }), Encoding.UTF8, "application/json"));
        //
        //     response.EnsureSuccessStatusCode();
        //
        //     var content1 = await response.Content.ReadAsStringAsync();
        //     var messages = JsonSerializer.Deserialize<List<RabbitMQMessage>>(content1)!.ToList();
        //
        //     var db = client.GetDatabase("webone");
        //     var collection = db.GetCollection<RabbitMQMessage>("rabbitmq");
        //
        //     foreach (var message in messages)
        //     {
        //         await collection.InsertOneAsync(message);
        //     }
        // });

        //
        // group.MapPost("", async (IMongoClient mongoClient, [FromBody] CreateQueueDto dto, UserManager<User> userManager, HttpContext httpContext) =>
        // {
        //     var user = await userManager.GetUserAsync(httpContext.User);
        //     if (user == null)
        //     {
        //         return Results.Unauthorized();
        //     }
        //
        //     if (!ObjectId.TryParse(dto.BrokerId, out var brokerObjectId))
        //     {
        //         return Results.BadRequest("Invalid Broker ID format.");
        //     }
        //
        //     var database = mongoClient.GetDatabase("webone");
        //     var queuesCollection = database.GetCollection<Queue>("queues");
        //
        //     var queue = new Queue
        //     {
        //         Name = dto.Name,
        //         BrokerId = brokerObjectId,
        //         UserId = user.Id
        //     };
        //
        //     await queuesCollection.InsertOneAsync(queue);
        //
        //     return Results.Ok();
        // });
        //
        // group.MapPut("/{id}", async (string id, [FromBody] UpdateQueueDto updateQueueDto, UserManager<User> userManager, HttpContext httpContext, IMongoClient mongoClient) =>
        // {
        //     if (!ObjectId.TryParse(id, out var objectId))
        //     {
        //         return Results.BadRequest("Invalid ID format.");
        //     }
        //
        //     var user = await userManager.GetUserAsync(httpContext.User);
        //     if (user == null)
        //     {
        //         return Results.Unauthorized();
        //     }
        //
        //     var database = mongoClient.GetDatabase("webone");
        //     var queuesCollection = database.GetCollection<Queue>("queues");
        //
        //     var filter = Builders<Queue>.Filter.And(
        //         Builders<Queue>.Filter.Eq(q => q.Id, objectId),
        //         Builders<Queue>.Filter.Eq(q => q.UserId, user.Id)
        //     );
        //
        //     var update = Builders<Queue>.Update
        //         .Set(q => q.Name, updateQueueDto.Name);
        //
        //     var result = await queuesCollection.UpdateOneAsync(filter, update);
        //
        //     return result.MatchedCount == 0 ? Results.NotFound() : Results.NoContent();
        // });
        //
        // group.MapDelete("{id}", async (IMongoClient mongoClient, UserManager<User> userManager, HttpContext httpContext, string id) =>
        // {
        //     if (!ObjectId.TryParse(id, out var objectId))
        //     {
        //         return Results.BadRequest("Invalid ID format.");
        //     }
        //
        //     var user = await userManager.GetUserAsync(httpContext.User);
        //     if (user == null)
        //     {
        //         return Results.Unauthorized();
        //     }
        //
        //     var database = mongoClient.GetDatabase("webone");
        //     var queuesCollection = database.GetCollection<Queue>("queues");
        //
        //     var filter = Builders<Queue>.Filter.And(
        //         Builders<Queue>.Filter.Eq(q => q.Id, objectId),
        //         Builders<Queue>.Filter.Eq(q => q.UserId, user.Id)
        //     );
        //
        //     var result = await queuesCollection.DeleteOneAsync(filter);
        //
        //     return result.DeletedCount == 0 ? Results.NotFound() : Results.Ok();
        // });

        return group;
    }
}