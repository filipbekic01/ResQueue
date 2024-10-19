using Dapper;
using Marten;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ResQueue.Dtos;
using ResQueue.Features.Messages.MoveMessage;
using ResQueue.Features.Messages.RequeueMessages;
using ResQueue.Models;
using ResQueue.Models.Postgres;
using Message = ResQueue.Models.Postgres.Message;

namespace ResQueue.Endpoints;

public static class MessageEndpoints
{
    public static void MapMessageEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("messages")
            .RequireAuthorization();

        group.MapGet("paginated",
            async (IDocumentSession documentSession,
                UserManager<User> userManager, HttpContext httpContext,
                [FromQuery] string brokerId, [FromQuery] long queueId, [FromQuery] int pageIndex = 0,
                int pageSize = 4) =>
            {
                // Validate filters
                pageSize = 15; // pageSize > 0 & pageSize <= 100 ? pageSize : 50;
                pageIndex = pageIndex >= 0 ? pageIndex : 0;

                // Get user
                var user = await userManager.FindByEmailAsync(httpContext.User.Identity.Name);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                // Validate broker
                if (!await documentSession.Query<Broker>()
                        .Where(x => x.Id == brokerId)
                        .Where(x => x.AccessList.Any(y => y.UserId == user.Id))
                        .Where(x => x.DeletedAt == null)
                        .AnyAsync())
                {
                    return Results.Unauthorized();
                }

                using (var db =
                       new NpgsqlConnection(
                           "host=localhost;port=5432;database=sandbox1;username=postgres;password=postgres;"))
                {
                    var offset = pageIndex * pageSize;
                    var sql = @"SELECT m.*, md.*
                                FROM transport.message m
                                JOIN transport.message_delivery md ON m.transport_message_id = md.transport_message_id
                                WHERE md.transport_message_id IS NOT NULL
                                    AND md.queue_id = @QueueId
                                ORDER BY md.transport_message_id 
                                LIMIT @PageSize OFFSET @Offset";

                    var sqlCount = @"SELECT COUNT(*) FROM transport.message_delivery where queue_id = @QueueId";
                    var total = await db.ExecuteScalarAsync<int>(sqlCount, new
                    {
                        QueueId = queueId
                    });

                    var messages = db.Query<Message, MessageDelivery, MessageDelivery>(
                        sql,
                        (message, messageDelivery) =>
                        {
                            messageDelivery.message = message;
                            return messageDelivery;
                        },
                        new { PageSize = pageSize, Offset = offset, QueueId = queueId },
                        splitOn: "message_delivery_id"
                    ).ToList();

                    return Results.Ok(new PaginatedResult<MessageDelivery>()
                    {
                        Items = messages,
                        PageIndex = pageIndex,
                        TotalPages = (int)Math.Ceiling((double)total / pageSize),
                        PageSize = pageSize,
                        TotalCount = total,
                    });
                }
            });

        group.MapPost("requeue",
            async (IRequeueMessagesFeature feature, HttpContext httpContext,
                [FromBody] RequeueMessagesDto dto) =>
            {
                var result = await feature.ExecuteAsync(new RequeueMessagesRequest(
                    dto
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem!);
            });

        group.MapPost("requeue-specific",
            async (IRequeueSpecificMessagesFeature feature, HttpContext httpContext,
                [FromBody] RequeueSpecificMessagesDto dto) =>
            {
                var result = await feature.ExecuteAsync(new RequeueSpecificMessagesRequest(
                    dto
                ));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem!);
            });

        // group.MapPost("publish",
        //     async (IPublishMessagesFeature publishMessagesFeature, HttpContext httpContext,
        //         [FromBody] PublishDto dto) =>
        //     {
        //         var result = await publishMessagesFeature.ExecuteAsync(new PublishMessagesFeatureRequest(
        //             ClaimsPrincipal: httpContext.User,
        //             Dto: dto
        //         ));
        //
        //         return result.IsSuccess
        //             ? Results.Ok(result.Value)
        //             : Results.Problem(result.Problem!);
        //     }); // Do not add retry filter

        // group.MapDelete("",
        //     async (IArchiveMessagesFeature archiveMessagesFeature, [FromBody] ArchiveMessagesDto dto,
        //         HttpContext httpContext, [FromQuery] bool purge = false) =>
        //     {
        //         var result = await archiveMessagesFeature.ExecuteAsync(new ArchiveMessagesFeatureRequest(
        //             ClaimsPrincipal: httpContext.User,
        //             Dto: dto,
        //             Purge: purge
        //         ));
        //
        //         return result.IsSuccess
        //             ? Results.Ok(result.Value)
        //             : Results.Problem(result.Problem!);
        //     }).AddRetryFilter();
    }
}