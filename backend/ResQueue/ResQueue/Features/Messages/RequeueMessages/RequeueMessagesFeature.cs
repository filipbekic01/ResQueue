using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ResQueue.Dtos;

namespace ResQueue.Features.Messages.RequeueMessages;

public record RequeueMessagesRequest(
    RequeueMessagesDto Dto
);

public record RequeueMessagesResponse();

public class RequeueMessagesFeature : IRequeueMessagesFeature
{
    public async Task<OperationResult<RequeueMessagesResponse>> ExecuteAsync(RequeueMessagesRequest request)
    {
        using var connection =
            new NpgsqlConnection("host=localhost;port=5432;database=sandbox1;username=postgres;password=postgres;");

        await connection.OpenAsync();

        var parameters = new DynamicParameters();
        parameters.Add("queue_name", request.Dto.QueueName);
        parameters.Add("source_queue_type", request.Dto.SourceQueueType);
        parameters.Add("target_queue_type", request.Dto.TargetQueueType);
        parameters.Add("message_count", request.Dto.MessageCount);
        // parameters.Add("delay", request.Dto.Delay); 
        parameters.Add("redelivery_count", request.Dto.RedeliveryCount);

        var result = await connection.ExecuteAsync("CALL requeue_messages()", parameters);

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
}