using Dapper;
using Microsoft.Extensions.Options;
using ResQueue.Enums;
using ResQueue.Factories;
using ResQueue.Models.Postgres;

namespace ResQueue.Features.Messages.GetMessages;

public record GetMessagesRequest(
    long QueueId,
    int PageIndex
);

public record GetMessagesResponse(
    PaginatedResult<MessageDelivery> Messages
);

public class GetMessagesFeature(
    IDatabaseConnectionFactory connectionFactory,
    IOptions<ResQueueOptions> options
) : IGetMessagesFeature
{
    public async Task<OperationResult<GetMessagesResponse>> ExecuteAsync(GetMessagesRequest request)
    {
        const int pageSize = 50;
        var pageIndex = request.PageIndex >= 0 ? request.PageIndex : 0;
        var offset = pageIndex * pageSize;

        var sql = GetSqlQueryText();
        var sqlCount = GetSqlQueryCountText();

        await using var connection = connectionFactory.CreateConnection();

        var total = await connection.ExecuteScalarAsync<int>(sqlCount, new
        {
            request.QueueId
        });

        var messages = connection.Query<Message, MessageDelivery, MessageDelivery>(
            sql,
            (message, messageDelivery) =>
            {
                messageDelivery.message = message;
                return messageDelivery;
            },
            new { PageSize = pageSize, Offset = offset, request.QueueId },
            splitOn: "message_delivery_id"
        ).ToList();

        return OperationResult<GetMessagesResponse>.Success(new GetMessagesResponse(
            new PaginatedResult<MessageDelivery>()
            {
                Items = messages,
                PageIndex = pageIndex,
                TotalPages = (int)Math.Ceiling((double)total / pageSize),
                PageSize = pageSize,
                TotalCount = total,
            }));
    }

    private string GetSqlQueryCountText()
    {
        return options.Value.SqlEngine switch
        {
            ResQueueSqlEngine.PostgreSql => "SELECT COUNT(*) FROM transport.message_delivery WHERE queue_id = @QueueId",
            ResQueueSqlEngine.SqlServer => "SELECT COUNT(*) FROM transport.message_delivery WHERE queue_id = @QueueId",
            _ => throw new NotSupportedException("Unsupported SQL Engine")
        };
    }

    private string GetSqlQueryText()
    {
        return options.Value.SqlEngine switch
        {
            ResQueueSqlEngine.PostgreSql => """
                                            SELECT m.*, md.*
                                            FROM transport.message_delivery md
                                            LEFT JOIN transport.message m ON m.transport_message_id = md.transport_message_id
                                            WHERE md.queue_id = @QueueId
                                            ORDER BY md.message_delivery_id 
                                            LIMIT @PageSize OFFSET @Offset
                                            """,
            ResQueueSqlEngine.SqlServer => """
                                           SELECT TOP (@PageSize) m.*, md.*
                                           FROM transport.message_delivery md
                                           LEFT JOIN transport.message m ON m.transport_message_id = md.transport_message_id
                                           WHERE md.queue_id = @QueueId
                                           ORDER BY md.message_delivery_id 
                                           OFFSET @Offset ROWS
                                           """,
            _ => throw new NotSupportedException("Unsupported SQL Engine")
        };
    }
}