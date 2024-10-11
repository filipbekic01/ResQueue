using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using ResQueue.Dtos;
using ResQueue.Features.Messages.ArchiveMessages;
using ResQueue.Features.Messages.CloneMessage;
using ResQueue.Features.Messages.CreateMessage;
using ResQueue.Features.Messages.PublishMessages;
using ResQueue.Features.Messages.ReviewMessages;
using ResQueue.Features.Messages.SyncMessages;
using ResQueue.Features.Messages.UpdateMessage;
using ResQueue.Filters;
using ResQueue.Mappers;
using ResQueue.Models;

namespace ResQueue.Endpoints;

public static class MessageEndpoints
{
    public static void MapMessageEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("messages")
            .RequireAuthorization();

        group.MapGet("",
            async (IMongoCollection<Message> messagesCollection,
                IMongoCollection<Broker> brokersCollection,
                IMongoCollection<Queue> queuesCollection,
                UserManager<User> userManager, HttpContext httpContext,
                [FromQuery] string brokerId, [FromQuery] string queueId,
                [FromQuery(Name = "ids[]")] string[] ids) =>
            {
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

                // Validate queue
                var queueFilter = Builders<Queue>.Filter.And(
                    Builders<Queue>.Filter.Eq(q => q.Id, ObjectId.Parse(queueId)),
                    Builders<Queue>.Filter.Eq(q => q.BrokerId, ObjectId.Parse(brokerId))
                );
                if (!await queuesCollection.Find(queueFilter).AnyAsync())
                {
                    return Results.Unauthorized();
                }

                // Get messages
                var filter = Builders<Message>.Filter.And(
                    Builders<Message>.Filter.In(q => q.Id, ids.Select(ObjectId.Parse)),
                    Builders<Message>.Filter.Eq(q => q.QueueId, ObjectId.Parse(queueId)),
                    Builders<Message>.Filter.Eq(q => q.DeletedAt, null)
                );

                var sort = Builders<Message>.Sort.Descending(q => q.Id);

                var messages = await messagesCollection.Find(filter).Sort(sort).ToListAsync();

                return Results.Ok(messages.Select(MessageMapper.ToDto));
            });

        group.MapGet("paginated",
            async (IMongoCollection<Message> messagesCollection,
                IMongoCollection<Broker> brokersCollection,
                IMongoCollection<Queue> queuesCollection,
                UserManager<User> userManager, HttpContext httpContext,
                [FromQuery] string brokerId, [FromQuery] string queueId, [FromQuery] int pageIndex = 0,
                int pageSize = 50) =>
            {
                // Validate filters
                pageSize = pageSize > 0 & pageSize <= 100 ? pageSize : 50;
                pageIndex = pageIndex >= 0 ? pageIndex : 0;

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

                // Validate queue
                var queueFilter = Builders<Queue>.Filter.And(
                    Builders<Queue>.Filter.Eq(q => q.Id, ObjectId.Parse(queueId)),
                    Builders<Queue>.Filter.Eq(q => q.BrokerId, ObjectId.Parse(brokerId))
                );
                if (!await queuesCollection.Find(queueFilter).AnyAsync())
                {
                    return Results.Unauthorized();
                }

                // Get messages
                var filter = Builders<Message>.Filter.And(
                    Builders<Message>.Filter.Eq(q => q.QueueId, ObjectId.Parse(queueId)),
                    Builders<Message>.Filter.Eq(q => q.DeletedAt, null)
                );

                var sort = Builders<Message>.Sort.Ascending(q => q.MessageOrder);

                var totalItems = await messagesCollection.CountDocumentsAsync(filter);
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var messages = await messagesCollection.Find(filter)
                    .Sort(sort)
                    .Skip((pageIndex) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

                return Results.Ok(new PaginatedResult<MessageDto>()
                {
                    Items = messages.Select(MessageMapper.ToDto).ToList(),
                    PageIndex = pageIndex,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    TotalCount = (int)totalItems,
                });
            });

        group.MapPost("",
            async (ICreateMessageFeature publishMessagesFeature, HttpContext httpContext,
                [FromBody] UpsertMessageDto dto) =>
            {
                var result = await publishMessagesFeature.ExecuteAsync(new CreateMessageFeatureRequest(
                    ClaimsPrincipal: httpContext.User,
                    Dto: dto
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem!);
            }).AddRetryFilter();

        group.MapPut("{id}",
            async (IUpdateMessageFeature feature, HttpContext httpContext,
                [FromBody] UpsertMessageDto dto, string id) =>
            {
                var result = await feature.ExecuteAsync(new UpdateMessageFeatureRequest(
                    ClaimsPrincipal: httpContext.User,
                    Id: id,
                    Dto: dto
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem!);
            }).AddRetryFilter();

        group.MapPost("sync",
            async (ISyncMessagesFeature syncMessagesFeature,
                HttpContext httpContext, [FromBody] SyncMessagesDto dto) =>
            {
                var result = await syncMessagesFeature.ExecuteAsync(new SyncMessagesFeatureRequest(
                    ClaimsPrincipal: httpContext.User,
                    QueueId: dto.QueueId
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem!);
            }); // Do not add retry filter

        group.MapPost("publish",
            async (IPublishMessagesFeature publishMessagesFeature, HttpContext httpContext,
                [FromBody] PublishDto dto) =>
            {
                var result = await publishMessagesFeature.ExecuteAsync(new PublishMessagesFeatureRequest(
                    ClaimsPrincipal: httpContext.User,
                    Dto: dto
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem!);
            }); // Do not add retry filter

        group.MapPost("review",
            async (IReviewMessagesFeature publishMessagesFeature, HttpContext httpContext,
                [FromBody] ReviewMessagesDto dto) =>
            {
                var result = await publishMessagesFeature.ExecuteAsync(new ReviewMessagesFeatureRequest(
                    ClaimsPrincipal: httpContext.User,
                    Dto: dto
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem!);
            }).AddRetryFilter();

        group.MapDelete("",
            async (IArchiveMessagesFeature archiveMessagesFeature, [FromBody] ArchiveMessagesDto dto,
                HttpContext httpContext, [FromQuery] bool purge = false) =>
            {
                var result = await archiveMessagesFeature.ExecuteAsync(new ArchiveMessagesFeatureRequest(
                    ClaimsPrincipal: httpContext.User,
                    Dto: dto,
                    Purge: purge
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem!);
            }).AddRetryFilter();

        group.MapPost("{id}/clone",
            async (ICloneMessageFeature feature, HttpContext httpContext, string id) =>
            {
                var result = await feature.ExecuteAsync(new CloneMessagesFeatureRequest(
                    ClaimsPrincipal: httpContext.User,
                    Id: id
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem!);
            }).AddRetryFilter();
    }
}