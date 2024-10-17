using Marten;
using Microsoft.AspNetCore.Mvc;
using ResQueue.Models;

namespace ResQueue.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("users");

        group.MapGet("basic",
                async (IDocumentSession documentSession,
                    [FromQuery(Name = "ids[]")] string[] ids) =>
                {
                    // var filter = Builders<User>.Filter.In(u => u.Id, ids.Select(ObjectId.Parse).ToList());
                    //
                    // var users = await usersCollection
                    //     .Find(filter)
                    //     .Project(u => new UserBasicDto
                    //     {
                    //         Id = u.Id.ToString(),
                    //         Email = u.Email!,
                    //         Avatar = u.Avatar,
                    //         FullName = u.FullName,
                    //         SubscriptionType = u.Subscription!.Type
                    //     })
                    //     .ToListAsync();

                    var users = await documentSession.Query<User>()
                        .Where(x => ids.Contains(x.Id))
                        .ToListAsync();

                    return Results.Ok(users);
                })
            .RequireAuthorization();
    }
}