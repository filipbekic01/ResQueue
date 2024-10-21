using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Npgsql;
using ResQueue.Dtos;

namespace ResQueue.Features.Messages.RequeueMessages;

public record RequeueMessagesRequest(
    RequeueMessagesDto Dto
);

public record RequeueMessagesResponse();

public class RequeueMessagesFeature(
    IOptions<Settings> settings
) : IRequeueMessagesFeature
{
    public async Task<OperationResult<RequeueMessagesResponse>> ExecuteAsync(RequeueMessagesRequest request)
    {
        await using var connection = new NpgsqlConnection(settings.Value.PostgreSQLConnectionString);

        await connection.OpenAsync();

        var result = await CallRoutineAsync(request, connection);

        if (result > 0)
        {
            return OperationResult<RequeueMessagesResponse>.Success(new RequeueMessagesResponse());
        }

        return OperationResult<RequeueMessagesResponse>.Failure(new ProblemDetails
        {
            Title = "Forbidden",
            Detail = "You can't adjust your own permission settings.",
            Status = StatusCodes.Status403Forbidden
        });
    }

    private static async Task<int?> CallRoutineAsync(RequeueMessagesRequest request, NpgsqlConnection connection)
    {
        var parameters = new DynamicParameters();
        parameters.Add("queue_name", request.Dto.QueueName);
        parameters.Add("source_queue_type", request.Dto.SourceQueueType);
        parameters.Add("target_queue_type", request.Dto.TargetQueueType);
        parameters.Add("message_count", request.Dto.MessageCount);
        parameters.Add("delay", request.Dto.Delay);
        parameters.Add("redelivery_count", request.Dto.RedeliveryCount);

        return await connection.QuerySingleAsync<int?>(
            "SELECT transport.requeue_messages(@queue_name, @source_queue_type, @target_queue_type, @message_count, @delay::interval, @redelivery_count)",
            parameters
        );
    }
}