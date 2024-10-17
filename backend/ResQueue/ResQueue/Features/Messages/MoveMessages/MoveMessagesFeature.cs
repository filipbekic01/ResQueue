using Dapper;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ResQueue.Dtos;

namespace ResQueue.Features.Messages.MoveMessage;

public record MoveMessagesRequest(
    MoveMessagesDto Dto
);

public record MoveMessagesResponse();

public class MoveMessagesFeature : IMoveMessagesFeature
{
    public async Task<OperationResult<MoveMessagesResponse>> ExecuteAsync(MoveMessagesRequest request)
    {
        using var connection =
            new NpgsqlConnection("host=localhost;port=5432;database=sandbox1;username=postgres;password=postgres;");

        await connection.OpenAsync();

        var parameters = new DynamicParameters();
        parameters.Add("message_delivery_id", request.Dto.MessageDeliveryId);
        parameters.Add("lock_id", NewId.NextGuid());
        parameters.Add("queue_name", request.Dto.QueueName);
        parameters.Add("queue_type", request.Dto.QueueType);

        var result = await connection.ExecuteAsync("CALL requeue_messages()", parameters);

        if (result > 0)
        {
            return OperationResult<MoveMessagesResponse>.Success(new MoveMessagesResponse());
        }

        return OperationResult<MoveMessagesResponse>.Failure(new ProblemDetails
        {
            Title = "Forbidden",
            Detail = "You can't adjust your own permission settings.",
            Status = StatusCodes.Status403Forbidden
        });
    }
}