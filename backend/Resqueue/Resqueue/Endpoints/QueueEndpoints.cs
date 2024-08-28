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
                    TotalMessages = x.TotalMessages,
                    IsFavorite = x.IsFavorite,
                    CreatedAt = x.CreatedAt
                }).ToList());
            });

        group.MapGet("paginated",
            async (IMongoCollection<Queue> collection, UserManager<User> userManager, HttpContext httpContext,
                [FromQuery] string brokerId,
                [FromQuery] string? sortField,
                [FromQuery] int? sortOrder,
                [FromQuery] int pageIndex = 0,
                [FromQuery] int pageSize = 50,
                [FromQuery] string search = "") =>
            {
                pageSize = pageSize > 0 & pageSize <= 100 ? pageSize : 50;
                pageIndex = pageIndex >= 0 ? pageIndex : 0;
                search = search.Trim();
                sortField = new[]
                {
                    "name", "synced", "messages"
                }.Contains(sortField)
                    ? sortField
                    : null;
                sortOrder = sortField is not null && sortOrder is 1 or -1 ? sortOrder : null;

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

                var sort = Builders<Queue>.Sort.Descending(q => q.IsFavorite);

                if (sortField is not null && sortOrder is not null)
                {
                    var secondarySort = sortOrder == 1
                        ? Builders<Queue>.Sort.Ascending($"RawData.{sortField}")
                        : Builders<Queue>.Sort.Descending($"RawData.{sortField}");

                    sort = Builders<Queue>.Sort.Combine(sort, secondarySort);
                }
                else
                {
                    sort = Builders<Queue>.Sort.Combine(sort, Builders<Queue>.Sort.Descending(q => q.Id));
                }

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
                        TotalMessages = x.TotalMessages,
                        IsFavorite = x.IsFavorite,
                        CreatedAt = x.CreatedAt
                    }).ToList(),
                    PageIndex = pageIndex,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    TotalCount = (int)totalItems,
                };

                return Results.Ok(result);
            });

        group.MapPost("{id}/favorite",
            async (IMongoCollection<Queue> collection, UserManager<User> userManager,
                HttpContext httpContext, [FromBody] FavoriteQueueDto dto,
                string id) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var filter = Builders<Queue>.Filter.And(
                    Builders<Queue>.Filter.Eq(q => q.Id, ObjectId.Parse(id)),
                    Builders<Queue>.Filter.Eq(q => q.UserId, user.Id)
                );

                var update = Builders<Queue>.Update.Set(q => q.IsFavorite, dto.IsFavorite);

                await collection.UpdateOneAsync(filter, update);

                return Results.Ok();
            });

        return group;
    }
}