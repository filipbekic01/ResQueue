using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Resqueue.Dtos;
using Resqueue.Features.Broker.SyncBroker;
using Resqueue.Features.Messages.ArchiveMessages;
using Resqueue.Features.Messages.CloneMessage;
using Resqueue.Features.Messages.CreateMessage;
using Resqueue.Features.Messages.PublishMessages;
using Resqueue.Features.Messages.ReviewMessages;
using Resqueue.Features.Messages.SyncMessages;
using Resqueue.Features.Messages.UpdateMessage;
using Resqueue.Filters;
using Resqueue.Mappers;
using Resqueue.Models;

namespace Resqueue.Endpoints;

public static class MessageEndpoints
{
    public static IEndpointRouteBuilder MapMessageEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("messages")
            .RequireAuthorization();

        group.MapGet("",
            async (IMongoCollection<Message> messagesCollection, UserManager<User> userManager, HttpContext httpContext,
                [FromQuery(Name = "ids[]")] string[] ids) =>
            {
                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var filter = Builders<Message>.Filter.And(
                    Builders<Message>.Filter.In(q => q.Id, ids.Select(ObjectId.Parse)),
                    Builders<Message>.Filter.Eq(q => q.UserId, user.Id),
                    Builders<Message>.Filter.Eq(q => q.DeletedAt, null)
                );

                var sort = Builders<Message>.Sort.Descending(q => q.Id);

                var messages = await messagesCollection.Find(filter).Sort(sort).ToListAsync();

                return Results.Ok(messages.Select(MessageMapper.ToDto));
            });

        group.MapGet("paginated",
            async (IMongoCollection<Message> messagesCollection, UserManager<User> userManager, HttpContext httpContext,
                [FromQuery] string queueId, [FromQuery] int pageIndex = 0,
                int pageSize = 50) =>
            {
                pageSize = pageSize > 0 & pageSize <= 100 ? pageSize : 50;
                pageIndex = pageIndex >= 0 ? pageIndex : 0;

                var user = await userManager.GetUserAsync(httpContext.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                if (!ObjectId.TryParse(queueId, out var queueIdObjectId))
                {
                    return Results.BadRequest("Invalid Broker ID format.");
                }

                var filter = Builders<Message>.Filter.And(
                    Builders<Message>.Filter.Eq(q => q.QueueId, queueIdObjectId),
                    Builders<Message>.Filter.Eq(q => q.UserId, user.Id),
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
            }).AddRetryFilter();

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
            }).AddRetryFilter();

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
                HttpContext httpContext) =>
            {
                var result = await archiveMessagesFeature.ExecuteAsync(new ArchiveMessagesFeatureRequest(
                    ClaimsPrincipal: httpContext.User,
                    Dto: dto
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

        return group;
    }
}