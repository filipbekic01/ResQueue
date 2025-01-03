using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ResQueue.Dtos.Messages;
using ResQueue.Enums;
using ResQueue.Factories;
using ResQueue.Features.Messages.GetSingleMessage.Models;
using ResQueue.Providers.DbConnectionProvider;

namespace ResQueue.Features.Messages.GetSingleMessage;

public record GetSingleMessageRequest(
    string TransportMessageId
);

public record GetSingleMessageResponse(
    MessageDeliveryDto Message
);

public class GetSingleMessageFeature(
    IDatabaseConnectionFactory connectionFactory,
    IDbConnectionProvider conn,
    IServiceProvider serviceProvider,
    IOptions<ResQueueOptions> resQueueOptions
) : IGetSingleMessageFeature
{
    public async Task<OperationResult<GetSingleMessageResponse>> ExecuteAsync(GetSingleMessageRequest request)
    {
        var sql = GetSqlQueryText();

        await using var connection = connectionFactory.CreateConnection();

        var message = connection.Query<MessageDto, MessageDeliveryDto, MessageDeliveryDto>(
            sql,
            (message, messageDelivery) =>
            {
                messageDelivery.Message = message;
                return messageDelivery;
            },
            new { request.TransportMessageId },
            splitOn: "MessageDeliveryId"
        ).FirstOrDefault();

        if (message is null)
        {
            return OperationResult<GetSingleMessageResponse>.Failure(new ProblemDetails
            {
                Status = 404,
                Title = "Not Found"
            });
        }

        // invoke each transformer
        var transformerTypes = resQueueOptions.Value.TransformerTypes;
        foreach (var transformerType in transformerTypes)
        {
            var transformer = (AbstractMessageTransformer)serviceProvider.GetRequiredService(transformerType);
            message = await transformer.TransformAsync(message);
        }

        // append additional data
        if (resQueueOptions.Value.AppendAdditionalData is not null)
        {
            var additionalData = resQueueOptions.Value.AppendAdditionalData(message);
            message.AdditionalData ??= new();
            additionalData.ToList().ForEach(x => message.AdditionalData[x.Key] = x.Value);
        }

        return OperationResult<GetSingleMessageResponse>.Success(new GetSingleMessageResponse(message));
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
                                           WHERE md.transport_message_id = @TransportMessageId
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
                                            WHERE md.TransportMessageId = @TransportMessageId;
                                            """,
            _ => throw new NotSupportedException("Unsupported SQL Engine")
        };
    }
}