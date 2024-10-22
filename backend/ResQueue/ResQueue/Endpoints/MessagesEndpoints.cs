using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Npgsql;
using ResQueue.Dtos;
using ResQueue.Features.Messages.MoveMessage;
using ResQueue.Features.Messages.RequeueMessages;
using ResQueue.Models.Postgres;
using Message = ResQueue.Models.Postgres.Message;

namespace ResQueue.Endpoints;

public static class MessagesEndpoints
{
    public static void MapMessageEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("messages");

        group.MapGet("paginated",
            async (IOptions<Settings> settings, [FromQuery] long queueId, [FromQuery] int pageIndex = 0,
                int pageSize = 4) =>
            {
                pageSize = 50;
                pageIndex = pageIndex >= 0 ? pageIndex : 0;

                await using var db = new NpgsqlConnection(settings.Value.PostgreSQLConnectionString);
                var offset = pageIndex * pageSize;
                var sql = @"SELECT m.*, md.*
                                FROM transport.message_delivery md
                                LEFT JOIN transport.message m ON m.transport_message_id = md.transport_message_id
                                WHERE md.queue_id = @QueueId
                                ORDER BY md.message_delivery_id 
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
            });

        group.MapPost("requeue",
            async (IRequeueMessagesFeature feature, [FromBody] RequeueMessagesDto dto) =>
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
    }
}