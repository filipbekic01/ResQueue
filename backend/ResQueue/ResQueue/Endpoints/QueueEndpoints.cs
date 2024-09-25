using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ResQueue.Dtos;
using ResQueue.Models;

namespace ResQueue.Endpoints;

public static class QueueEndpoints
{
    public static void MapQueueEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("queues")
            .RequireAuthorization();

        group.MapGet("",
            async (IMongoCollection<Queue> queuesCollection, IMongoCollection<Broker> brokersCollection,
                UserManager<User> userManager, HttpContext httpContext,
                [FromQuery(Name = "ids[]")] string[] ids) =>
            {
                // Get user
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                // Get queue broker id
                var queueFilter = Builders<Queue>.Filter.In(b => b.Id, ids.Select(ObjectId.Parse).ToList());
                var brokerId = await queuesCollection.Find(queueFilter).Project(x => x.BrokerId).SingleAsync();

                // Validate broker
                var brokerFilter = Builders<Broker>.Filter.And(
                    Builders<Broker>.Filter.Eq(b => b.Id, brokerId),
                    Builders<Broker>.Filter.ElemMatch(b => b.AccessList, a => a.UserId == user.Id),
                    Builders<Broker>.Filter.Eq(b => b.DeletedAt, null)
                );
                if (!await brokersCollection.Find(brokerFilter).AnyAsync())
                {
                    return Results.Unauthorized();
                }

                // Get queues
                var queuesFilter = Builders<Queue>.Filter.In(q => q.Id, ids.Select(ObjectId.Parse).ToList());
                var queues = await queuesCollection.Find(queuesFilter).ToListAsync();

                return Results.Ok(queues.Select(x => new QueueDto()
                {
                    Id = x.Id.ToString(),
                    RawData = JsonHelper.ConvertBsonToJson(x.RawData),
                    TotalMessages = x.TotalMessages,
                    IsFavorite = x.IsFavorite,
                    CreatedAt = x.CreatedAt
                }).ToList());
            });

        group.MapGet("paginated",
            async (IMongoCollection<Queue> queuesCollection, IMongoCollection<Broker> brokersCollection,
                UserManager<User> userManager, HttpContext httpContext,
                [FromQuery] string brokerId,
                [FromQuery] string? sortField,
                [FromQuery] int? sortOrder,
                [FromQuery] int pageIndex = 0,
                [FromQuery] int pageSize = 50,
                [FromQuery] string search = "") =>
            {
                // Validate inputs
                pageSize = pageSize > 0 & pageSize <= 100 ? pageSize : 50;
                pageIndex = pageIndex >= 0 ? pageIndex : 0;
                search = search.Trim();
                sortField = new[]
                {
                    "name", "messages"
                }.Contains(sortField)
                    ? sortField
                    : null;
                sortOrder = sortField is not null && sortOrder is 1 or -1 ? sortOrder : null;

                // Get user
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                // Validate broker
                var brokerFilter = Builders<Broker>.Filter.And(
                    Builders<Broker>.Filter.Eq(b => b.Id, ObjectId.Parse(brokerId)),
                    Builders<Broker>.Filter.ElemMatch(b => b.AccessList, a => a.UserId == user.Id),
                    Builders<Broker>.Filter.Eq(b => b.DeletedAt, null)
                );
                if (!await brokersCollection.Find(brokerFilter).AnyAsync())
                {
                    return Results.Unauthorized();
                }

                // Get queue
                var filters = new List<FilterDefinition<Queue>>
                {
                    Builders<Queue>.Filter.Eq(q => q.BrokerId, ObjectId.Parse(brokerId))
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
                        ? Builders<Queue>.Sort.Ascending(x => x.RawData[sortField])
                        : Builders<Queue>.Sort.Descending(x => x.RawData[sortField]);

                    if (sortField.Equals("messages", StringComparison.CurrentCultureIgnoreCase))
                    {
                        secondarySort = sortOrder == 1
                            ? Builders<Queue>.Sort.Ascending(x => x.TotalMessages)
                            : Builders<Queue>.Sort.Descending(x => x.TotalMessages);
                    }

                    sort = Builders<Queue>.Sort.Combine(sort, secondarySort);
                }
                else
                {
                    sort = Builders<Queue>.Sort.Combine(sort, Builders<Queue>.Sort.Descending(q => q.Id));
                }

                var totalItems = await queuesCollection.CountDocumentsAsync(filter);
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var queues = await queuesCollection.Find(filter)
                    .Sort(sort)
                    .Skip((pageIndex) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

                var result = new PaginatedResult<QueueDto>
                {
                    Items = queues.Select(x => new QueueDto()
                    {
                        Id = x.Id.ToString(),
                        RawData = JsonHelper.ConvertBsonToJson(x.RawData),
                        TotalMessages = x.TotalMessages,
                        Messages = x.Messages,
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
            async (IMongoCollection<Queue> queuesCollection, IMongoCollection<Broker> brokersCollection,
                UserManager<User> userManager,
                HttpContext httpContext, [FromBody] FavoriteQueueDto dto,
                string id) =>
            {
                // Get user
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                // Get queue broker id
                var queueFilter = Builders<Queue>.Filter.And(
                    Builders<Queue>.Filter.Eq(b => b.Id, ObjectId.Parse(id))
                );

                var brokerId = await queuesCollection.Find(queueFilter).Project(x => x.BrokerId).SingleAsync();

                // Validate broker
                var brokerFilter = Builders<Broker>.Filter.And(
                    Builders<Broker>.Filter.Eq(b => b.Id, brokerId),
                    Builders<Broker>.Filter.ElemMatch(b => b.AccessList, a => a.UserId == user.Id),
                    Builders<Broker>.Filter.Eq(b => b.DeletedAt, null)
                );

                if (!await brokersCollection.Find(brokerFilter).AnyAsync())
                {
                    return Results.Unauthorized();
                }

                // Favorite queue
                var filter = Builders<Queue>.Filter.And(
                    Builders<Queue>.Filter.Eq(q => q.Id, ObjectId.Parse(id))
                );

                var update = Builders<Queue>.Update.Set(q => q.IsFavorite, dto.IsFavorite);

                await queuesCollection.UpdateOneAsync(filter, update);

                return Results.Ok();
            });
    }
}