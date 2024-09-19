using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ResQueue.Dtos;
using ResQueue.Models;

namespace ResQueue.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("users");

        group.MapGet("basic",
                async (IMongoCollection<User> usersCollection,
                    [FromQuery(Name = "ids[]")] string[] ids) =>
                {
                    var filter = Builders<User>.Filter.In(u => u.Id, ids.Select(ObjectId.Parse).ToList());

                    var users = await usersCollection
                        .Find(filter)
                        .Project(u => new UserBasicDto
                        {
                            Id = u.Id.ToString(),
                            Email = u.Email!,
                            Avatar = u.Avatar,
                            FullName = u.FullName,
                            SubscriptionType = u.Subscription!.Type
                        })
                        .ToListAsync();

                    return Results.Ok(users);
                })
            .RequireAuthorization();
    }
}