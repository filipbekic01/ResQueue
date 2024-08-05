using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using WebOne.Dtos;
using WebOne.Models;
using WebOne.Models.MongoDB;

namespace WebOne.Endpoints;

public static class BrokerEndpoints
{
    public static IEndpointRouteBuilder MapBrokerEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("brokers")
            .RequireAuthorization();

        group.MapGet("",
            async (IMongoClient mongoClient, UserManager<User> userManager, HttpContext httpContext) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var database = mongoClient.GetDatabase("webone");
                var brokersCollection = database.GetCollection<Broker>("brokers");

                var filter = Builders<Broker>.Filter.Eq(b => b.UserId, user.Id);
                var brokers = await brokersCollection.Find(filter).ToListAsync();
                var dtos = brokers.Select(x => new BrokerDto(
                    x.Id.ToString(),
                    x.Name,
                    x.Port,
                    x.Url
                ));

                return Results.Ok(dtos);
            });

        group.MapPost("",
            async (IMongoClient mongoClient, [FromBody] CreateBrokerDto dto, UserManager<User> userManager,
                HttpContext httpContext) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                byte[] authBytes = Encoding.UTF8.GetBytes($"{dto.Username}:{dto.Password}");
                string authBase64 = Convert.ToBase64String(authBytes);

                var database = mongoClient.GetDatabase("webone");
                var brokersCollection = database.GetCollection<Broker>("brokers");

                var broker = new Broker
                {
                    Name = dto.Name,
                    Auth = authBase64,
                    Port = dto.Port,
                    Url = dto.Url,
                    UserId = user.Id
                };

                await brokersCollection.InsertOneAsync(broker);

                return Results.Ok();
            });

        group.MapPut("/{id}",
            async (string id, [FromBody] UpdateBrokerDto updateBrokerDto, UserManager<User> userManager,
                HttpContext httpContext, IMongoClient mongoClient) =>
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

                var database = mongoClient.GetDatabase("webone");
                var brokersCollection = database.GetCollection<Broker>("brokers");

                var filter = Builders<Broker>.Filter.And(
                    Builders<Broker>.Filter.Eq(b => b.Id, objectId),
                    Builders<Broker>.Filter.Eq(b => b.UserId, user.Id)
                );

                var update = Builders<Broker>.Update
                    .Set(b => b.Auth, $"{updateBrokerDto.Username}:{updateBrokerDto.Password}")
                    .Set(b => b.Port, updateBrokerDto.Port)
                    .Set(b => b.Url, updateBrokerDto.Url);

                var result = await brokersCollection.UpdateOneAsync(filter, update);

                return result.MatchedCount == 0 ? Results.NotFound() : Results.NoContent();
            });

        group.MapDelete("{id}",
            async (IMongoClient mongoClient, UserManager<User> userManager, HttpContext httpContext, string id) =>
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

                var database = mongoClient.GetDatabase("webone");
                var brokersCollection = database.GetCollection<Broker>("brokers");

                var filter = Builders<Broker>.Filter.And(
                    Builders<Broker>.Filter.Eq(b => b.Id, objectId),
                    Builders<Broker>.Filter.Eq(b => b.UserId, user.Id)
                );

                var result = await brokersCollection.DeleteOneAsync(filter);

                return result.DeletedCount == 0 ? Results.NotFound() : Results.Ok();
            });

        return group;
    }
}