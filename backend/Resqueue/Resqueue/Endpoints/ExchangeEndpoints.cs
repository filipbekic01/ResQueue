using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Models;

namespace Resqueue.Endpoints;

public static class ExchangeEndpoints
{
    public static void MapExchangeEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("exchanges")
            .RequireAuthorization();

        group.MapGet("{brokerId}",
            async (IMongoCollection<Exchange> collection, UserManager<User> userManager, HttpContext httpContext,
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

                var filter = Builders<Exchange>.Filter.And(
                    Builders<Exchange>.Filter.Eq(q => q.UserId, user.Id),
                    Builders<Exchange>.Filter.Eq(q => q.BrokerId, brokerObjectId)
                );

                var exchanges = await collection.Find(filter).ToListAsync();

                return Results.Ok(exchanges.Select(q => new ExchangeDto()
                {
                    Id = q.Id.ToString(),
                    RawData = q.RawData.ToString()
                }));
            });
    }
}