using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        group.MapGet("",
            async (IMongoCollection<Queue> collection, UserManager<User> userManager, HttpContext httpContext,
                [FromQuery(Name = "ids[]")] string[] ids) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var filter = Builders<Queue>.Filter.And(
                    Builders<Queue>.Filter.In(q => q.Id, ids.Select(ObjectId.Parse).ToList()),
                    Builders<Queue>.Filter.Eq(q => q.UserId, user.Id)
                );

                var queues = await collection.Find(filter).ToListAsync();

                return Results.Ok(queues.Select(x => new QueueDto()
                {
                    Id = x.Id.ToString(),
                    RawData = x.RawData.ToString(),
                    CreatedAt = x.CreatedAt
                }).ToList());
            });

        group.MapGet("paginated",
            async (IMongoCollection<Queue> collection, UserManager<User> userManager, HttpContext httpContext,
                [FromQuery] string brokerId, [FromQuery] int pageIndex = 0,
                int pageSize = 50, [FromQuery] string search = "") =>
            {
                pageSize = pageSize > 0 & pageSize <= 100 ? pageSize : 50;
                pageIndex = pageIndex >= 0 ? pageIndex : 0;
                search = search.Trim();

                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                if (!ObjectId.TryParse(brokerId, out var brokerObjectId))
                {
                    return Results.BadRequest("Invalid Broker ID format.");
                }

                var filters = new List<FilterDefinition<Queue>>
                {
                    Builders<Queue>.Filter.Eq(q => q.UserId, user.Id),
                    Builders<Queue>.Filter.Eq(q => q.BrokerId, brokerObjectId)
                };

                if (!string.IsNullOrWhiteSpace(search))
                {
                    filters.Add(Builders<Queue>.Filter.Regex("RawData.name", new BsonRegularExpression(search, "i")));
                }

                var filter = Builders<Queue>.Filter.And(filters);

                var sort = Builders<Queue>.Sort.Descending(q => q.Id);

                var totalItems = await collection.CountDocumentsAsync(filter);
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var queues = await collection.Find(filter)
                    .Sort(sort)
                    .Skip((pageIndex) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

                var result = new PaginatedResult<QueueDto>
                {
                    Items = queues.Select(x => new QueueDto()
                    {
                        Id = x.Id.ToString(),
                        RawData = x.RawData.ToString(),
                        CreatedAt = x.CreatedAt
                    }).ToList(),
                    PageIndex = pageIndex,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    TotalCount = (int)totalItems,
                };

                return Results.Ok(result);
            });

        return group;
    }
}