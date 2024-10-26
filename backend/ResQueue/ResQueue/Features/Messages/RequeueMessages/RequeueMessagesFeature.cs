using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ResQueue.Dtos.Messages;
using ResQueue.Enums;
using ResQueue.Factories;

namespace ResQueue.Features.Messages.RequeueMessages;

public record RequeueMessagesRequest(
    RequeueMessagesDto Dto
);

public record RequeueMessagesResponse();

public class RequeueMessagesFeature(
    IDatabaseConnectionFactory connectionFactory,
    IOptions<ResQueueOptions> options
) : IRequeueMessagesFeature
{
    public async Task<OperationResult<RequeueMessagesResponse>> ExecuteAsync(RequeueMessagesRequest request)
    {
        await using var connection = connectionFactory.CreateConnection();

        await connection.OpenAsync();

        await CallRoutineAsync(request, connection);

        return OperationResult<RequeueMessagesResponse>.Success(new RequeueMessagesResponse());
    }

    private async Task<int?> CallRoutineAsync(RequeueMessagesRequest request, DbConnection connection)
    {
        var parameters = new DynamicParameters();
        string commandText;

        switch (options.Value.SqlEngine)
        {
            case ResQueueSqlEngine.PostgreSql:
                commandText =
                    "SELECT transport.requeue_messages(@queue_name, @source_queue_type, @target_queue_type, @message_count, @delay::interval, @redelivery_count)";
                parameters.Add("queue_name", request.Dto.QueueName);
                parameters.Add("source_queue_type", request.Dto.SourceQueueType);
                parameters.Add("target_queue_type", request.Dto.TargetQueueType);
                parameters.Add("message_count", request.Dto.MessageCount);
                parameters.Add("delay", request.Dto.Delay);
                parameters.Add("redelivery_count", request.Dto.RedeliveryCount);
                break;

            case ResQueueSqlEngine.SqlServer:
                commandText =
                    "EXEC transport.requeue_messages @queue_name, @source_queue_type, @target_queue_type, @message_count, @delay, @redelivery_count";
                parameters.Add("queue_name", request.Dto.QueueName);
                parameters.Add("source_queue_type", request.Dto.SourceQueueType);
                parameters.Add("target_queue_type", request.Dto.TargetQueueType);
                parameters.Add("message_count", request.Dto.MessageCount);
                parameters.Add("delay", request.Dto.Delay);
                parameters.Add("redelivery_count", request.Dto.RedeliveryCount);
                break;

            default:
                throw new NotSupportedException();
        }

        return await connection.QuerySingleAsync<int?>(commandText, parameters);
    }
}