using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using ResQueue.Dtos.Messages;
using ResQueue.Enums;
using ResQueue.Factories;

namespace ResQueue.Features.Messages.RequeueSpecificMessages;

public record RequeueSpecificMessagesRequest(
    RequeueSpecificMessagesDto Dto
);

public record RequeueSpecificMessagesResponse();

public class RequeueSpecificMessagesFeature(
    IDatabaseConnectionFactory connectionFactory,
    IOptions<ResQueueOptions> options
) : IRequeueSpecificMessagesFeature
{
    public async Task<OperationResult<RequeueSpecificMessagesResponse>> ExecuteAsync(
        RequeueSpecificMessagesRequest request)
    {
        await using var connection = connectionFactory.CreateConnection();

        await connection.OpenAsync();

        if (request.Dto.Transactional)
        {
            await using var transaction = await connection.BeginTransactionAsync();

            foreach (var messageDeliveryId in request.Dto.MessageDeliveryIds)
            {
                await CallRoutineAsync(request, messageDeliveryId, connection);
            }

            await transaction.CommitAsync();
        }
        else
        {
            foreach (var messageDeliveryIds in request.Dto.MessageDeliveryIds)
            {
                await CallRoutineAsync(request, messageDeliveryIds, connection);
            }
        }

        return OperationResult<RequeueSpecificMessagesResponse>.Success(
            new RequeueSpecificMessagesResponse());
    }


    private async Task CallRoutineAsync(RequeueSpecificMessagesRequest request, long deliveryMessageId,
        DbConnection connection)
    {
        var parameters = new DynamicParameters();
        string commandText;

        switch (options.Value.SqlEngine)
        {
            case ResQueueSqlEngine.Postgres:
                commandText =
                    $"SELECT {options.Value.Schema}.requeue_message(@message_delivery_id, @target_queue_type, @delay::text::interval, @redelivery_count)";
                parameters.Add("message_delivery_id", deliveryMessageId);
                parameters.Add("target_queue_type", request.Dto.TargetQueueType);
                parameters.Add("delay", request.Dto.Delay);
                parameters.Add("redelivery_count", request.Dto.RedeliveryCount);
                break;

            case ResQueueSqlEngine.SqlServer:
                commandText =
                    $"EXEC {options.Value.Schema}.RequeueMessage @messageDeliveryId, @targetQueueType, @delay, @redeliveryCount";
                parameters.Add("messageDeliveryId", deliveryMessageId);
                parameters.Add("targetQueueType", request.Dto.TargetQueueType);
                parameters.Add("delay", request.Dto.Delay);
                parameters.Add("redeliveryCount", request.Dto.RedeliveryCount);
                break;

            default:
                throw new NotSupportedException();
        }

        await connection.ExecuteScalarAsync(commandText, parameters);
    }
}