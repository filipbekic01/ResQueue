using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Models;

namespace Resqueue.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("users");

        group.MapGet("basic",
                async (IMongoCollection<User> usersCollection,
                    [FromQuery(Name = "ids[]")] string[] ids) =>
                {
                    var filter = Builders<User>.Filter.In(u => u.Id, ids.Select(id => ObjectId.Parse(id)).ToList());

                    var users = await usersCollection
                        .Find(filter)
                        .Project(u => new UserBasicDto
                        {
                            Id = u.Id.ToString(),
                            Email = u.Email,
                            FullName = u.FullName
                        })
                        .ToListAsync();

                    return Results.Ok(users);
                })
            .RequireAuthorization();

        return group;
    }
}