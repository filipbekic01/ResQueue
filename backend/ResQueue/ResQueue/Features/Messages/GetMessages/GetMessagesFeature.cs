using Dapper;
using Microsoft.Extensions.Options;
using ResQueue.Dtos.Messages;
using ResQueue.Enums;
using ResQueue.Factories;
using ResQueue.Providers.DbConnectionProvider;

namespace ResQueue.Features.Messages.GetMessages;

public record GetMessagesRequest(
    long QueueId,
    int PageIndex
);

public record GetMessagesResponse(
    PaginatedResult<MessageDeliveryDto> Messages
);

public class GetMessagesFeature(
    IDatabaseConnectionFactory connectionFactory,
    IDbConnectionProvider conn,
    IOptions<ResQueueOptions> resQueueOptions
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

        var messages = connection.Query<MessageDto, MessageDeliveryDto, MessageDeliveryDto>(
            sql,
            (message, messageDelivery) =>
            {
                messageDelivery.Message = message;
                return messageDelivery;
            },
            new { PageSize = pageSize, Offset = offset, request.QueueId },
            splitOn: "MessageDeliveryId"
        ).ToList();

        if (resQueueOptions.Value.AppendAdditionalData is not null)
        {
            foreach (var message in messages)
            {
                var additionalData = resQueueOptions.Value.AppendAdditionalData(message);
                message.AdditionalData = additionalData;
            }
        }

        return OperationResult<GetMessagesResponse>.Success(new GetMessagesResponse(
            new PaginatedResult<MessageDeliveryDto>()
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
        return conn.SqlEngine switch
        {
            ResQueueSqlEngine.Postgres =>
                $"SELECT COUNT(*) FROM {conn.Schema}.message_delivery WHERE queue_id = @QueueId",
            ResQueueSqlEngine.SqlServer =>
                $"SELECT COUNT(*) FROM {conn.Schema}.MessageDelivery WHERE QueueId = @QueueId",
            _ => throw new NotSupportedException("Unsupported SQL Engine")
        };
    }

    private string GetSqlQueryText()
    {
        return conn.SqlEngine switch
        {
            ResQueueSqlEngine.Postgres => $"""
                                           SELECT 
                                               m.transport_message_id AS TransportMessageId,
                                               m.content_type AS ContentType,
                                               m.message_type AS MessageType,
                                               m.body AS Body,
                                               m.binary_body AS BinaryBody,
                                               m.message_id AS MessageId,
                                               m.correlation_id AS CorrelationId,
                                               m.conversation_id AS ConversationId,
                                               m.request_id AS RequestId,
                                               m.initiator_id AS InitiatorId,
                                               m.scheduling_token_id AS SchedulingTokenId,
                                               m.source_address AS SourceAddress,
                                               m.destination_address AS DestinationAddress,
                                               m.response_address AS ResponseAddress,
                                               m.fault_address AS FaultAddress,
                                               m.sent_time AS SentTime,
                                               m.headers AS Headers,
                                               m.host AS Host,
                                               md.message_delivery_id AS MessageDeliveryId,
                                               md.transport_message_id AS TransportMessageId,
                                               md.queue_id AS QueueId,
                                               md.priority AS Priority,
                                               md.enqueue_time AS EnqueueTime,
                                               md.expiration_time AS ExpirationTime,
                                               md.partition_key AS PartitionKey,
                                               md.routing_key AS RoutingKey,
                                               md.consumer_id AS ConsumerId,
                                               md.lock_id AS LockId,
                                               md.delivery_count AS DeliveryCount,
                                               md.max_delivery_count AS MaxDeliveryCount,
                                               md.last_delivered AS LastDelivered,
                                               md.transport_headers AS TransportHeaders
                                           FROM {conn.Schema}.message_delivery md
                                           LEFT JOIN {conn.Schema}.message m ON m.transport_message_id = md.transport_message_id
                                           WHERE md.queue_id = @QueueId
                                           ORDER BY md.message_delivery_id 
                                           LIMIT @PageSize OFFSET @Offset
                                           """,
            ResQueueSqlEngine.SqlServer => $"""
                                            SELECT 
                                                m.TransportMessageId,
                                                m.ContentType,
                                                m.MessageType,
                                                m.Body,
                                                m.BinaryBody,
                                                m.MessageId,
                                                m.CorrelationId,
                                                m.ConversationId,
                                                m.RequestId,
                                                m.InitiatorId,
                                                m.SchedulingTokenId,
                                                m.SourceAddress,
                                                m.DestinationAddress,
                                                m.ResponseAddress,
                                                m.FaultAddress,
                                                m.SentTime,
                                                m.Headers,
                                                m.Host,
                                                md.MessageDeliveryId,
                                                md.TransportMessageId,
                                                md.QueueId,
                                                md.Priority,
                                                md.EnqueueTime,
                                                md.ExpirationTime,
                                                md.PartitionKey,
                                                md.RoutingKey,
                                                md.ConsumerId,
                                                md.LockId,
                                                md.DeliveryCount,
                                                md.MaxDeliveryCount,
                                                md.LastDelivered,
                                                md.TransportHeaders
                                            FROM {conn.Schema}.MessageDelivery md
                                            LEFT JOIN {conn.Schema}.Message m ON m.TransportMessageId = md.TransportMessageId
                                            WHERE md.QueueId = @QueueId
                                            ORDER BY md.MessageDeliveryId 
                                            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                                            """,
            _ => throw new NotSupportedException("Unsupported SQL Engine")
        };
    }
}