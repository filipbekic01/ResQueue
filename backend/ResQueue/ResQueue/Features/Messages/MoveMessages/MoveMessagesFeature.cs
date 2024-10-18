using Dapper;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ResQueue.Dtos;

namespace ResQueue.Features.Messages.MoveMessage;

public record MoveMessagesRequest(
    MoveMessagesDto Dto
);

public record MoveMessagesResponse(
    int SucceededCount
);

public class MoveMessagesFeature : IMoveMessagesFeature
{
    public async Task<OperationResult<MoveMessagesResponse>> ExecuteAsync(MoveMessagesRequest request)
    {
        await using var connection =
            new NpgsqlConnection("host=localhost;port=5432;database=sandbox1;username=postgres;password=postgres;");

        await connection.OpenAsync();

        if (request.Dto.Transactional)
        {
            await using var transaction = await connection.BeginTransactionAsync();

            foreach (var message in request.Dto.Messages)
            {
                await CallRoutine(request, message, connection);
            }

            await transaction.CommitAsync();

            return OperationResult<MoveMessagesResponse>.Success(new MoveMessagesResponse(request.Dto.Messages.Length));
        }

        var succeededCount = 0;
        foreach (var message in request.Dto.Messages)
        {
            if (await CallRoutine(request, message, connection) > 0)
            {
                succeededCount++;
            }
        }

        return OperationResult<MoveMessagesResponse>.Success(new MoveMessagesResponse(succeededCount));
    }

    private static async Task<int?> CallRoutine(MoveMessagesRequest request, MoveMessageDeliveryDto message,
        NpgsqlConnection connection)
    {
        var parameters = new DynamicParameters();
        parameters.Add("message_delivery_id", message.MessageDeliveryId);
        parameters.Add("queue_name", request.Dto.QueueName);
        parameters.Add("queue_type", request.Dto.QueueType);
        parameters.Add("headers", message.Headers);
        parameters.Add("lock_id", message.LockId);

        return await connection.QuerySingleAsync<int?>(
            $"SELECT transport.move_message(@message_delivery_id, @lock_id::uuid, @queue_name, @queue_type, @headers::jsonb)",
            parameters);
    }
}