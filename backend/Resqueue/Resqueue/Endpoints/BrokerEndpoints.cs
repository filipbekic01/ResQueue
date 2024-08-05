using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Models;
using Resqueue.Models.MongoDB;

namespace Resqueue.Endpoints;

public static class BrokerEndpoints
{
    public static IEndpointRouteBuilder MapBrokerEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("brokers")
            .RequireAuthorization();

        group.MapGet("",
            async (IMongoCollection<Broker> collection, UserManager<User> userManager, HttpContext httpContext) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var filter = Builders<Broker>.Filter.Eq(b => b.UserId, user.Id);
                var brokers = await collection.Find(filter).ToListAsync();
                var dtos = brokers.Select(x => new BrokerDto(
                    x.Id.ToString(),
                    x.Name,
                    x.Port,
                    x.Url
                ));

                return Results.Ok(dtos);
            });

        group.MapPost("",
            async (IMongoCollection<Broker> collection, [FromBody] CreateBrokerDto dto, UserManager<User> userManager,
                HttpContext httpContext) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                byte[] authBytes = Encoding.UTF8.GetBytes($"{dto.Username}:{dto.Password}");
                string authBase64 = Convert.ToBase64String(authBytes);

                var broker = new Broker
                {
                    Name = dto.Name,
                    Auth = authBase64,
                    Port = dto.Port,
                    Url = dto.Url,
                    UserId = user.Id
                };

                await collection.InsertOneAsync(broker);

                return Results.Ok();
            });

        group.MapPost("{brokerId}/sync",
            async (IMongoCollection<Queue> queuesCollection, IMongoCollection<Broker> brokersCollection,
                [FromBody] CreateBrokerDto dto, UserManager<User> userManager,
                HttpContext httpContext, IHttpClientFactory httpClientFactory, string brokerId) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var filter = Builders<Broker>.Filter.Eq(b => b.Id, ObjectId.Parse(brokerId));
                var broker = await brokersCollection.Find(filter).FirstOrDefaultAsync();
                if (broker == null)
                {
                    return Results.Unauthorized();
                }

                var http = httpClientFactory.CreateClient();

                http.BaseAddress = new Uri($"{broker.Url}:{broker.Port}");

                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", broker.Auth);
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await http.GetAsync($"/api/queues");
                response.EnsureSuccessStatusCode();

                var content1 = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(content1);
                var newQueues = new List<Queue>();
                JsonElement root = document.RootElement;

                foreach (JsonElement element in root.EnumerateArray())
                {
                    // Check if the JSON object has a "Name" property
                    if (element.TryGetProperty("name", out JsonElement nameProperty))
                    {
                        var bsonDocument = BsonDocument.Parse(element.GetRawText());
                        var q = new Queue
                        {
                            BrokerId = new ObjectId(brokerId),
                            UserId = user.Id,
                            Name = nameProperty.ToString(),
                            Data = bsonDocument
                        };

                        newQueues.Add(q);
                        // Console.WriteLine($"Object has a Name property with value: {nameProperty.GetString()}");
                    }
                }

                await queuesCollection.InsertManyAsync(newQueues);

                return Results.Ok();
            });

        group.MapPut("/{id}",
            async (string id, [FromBody] UpdateBrokerDto updateBrokerDto, UserManager<User> userManager,
                HttpContext httpContext, IMongoCollection<Broker> collection) =>
            {
                if (!ObjectId.TryParse(id, out var objectId))
                {
                    return Results.BadRequest("Invalid ID format.");
                }

                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var filter = Builders<Broker>.Filter.And(
                    Builders<Broker>.Filter.Eq(b => b.Id, objectId),
                    Builders<Broker>.Filter.Eq(b => b.UserId, user.Id)
                );

                var update = Builders<Broker>.Update
                    .Set(b => b.Auth, $"{updateBrokerDto.Username}:{updateBrokerDto.Password}")
                    .Set(b => b.Port, updateBrokerDto.Port)
                    .Set(b => b.Url, updateBrokerDto.Url);

                var result = await collection.UpdateOneAsync(filter, update);

                return result.MatchedCount == 0 ? Results.NotFound() : Results.NoContent();
            });

        group.MapDelete("{id}",
            async (IMongoCollection<Broker> collection, UserManager<User> userManager, HttpContext httpContext,
                string id) =>
            {
                if (!ObjectId.TryParse(id, out var objectId))
                {
                    return Results.BadRequest("Invalid ID format.");
                }

                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var filter = Builders<Broker>.Filter.And(
                    Builders<Broker>.Filter.Eq(b => b.Id, objectId),
                    Builders<Broker>.Filter.Eq(b => b.UserId, user.Id)
                );

                var result = await collection.DeleteOneAsync(filter);

                return result.DeletedCount == 0 ? Results.NotFound() : Results.Ok();
            });

        return group;
    }
}