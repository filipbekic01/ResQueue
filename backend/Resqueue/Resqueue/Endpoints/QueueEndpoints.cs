using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Models;

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
                    RawData = q.RawData.ToString()
                }));
            });

        return group;
    }
}