using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Options;
using ResQueue.Dtos.Messages;
using ResQueue.Enums;
using ResQueue.Factories;

namespace ResQueue.Features.Messages.DeleteMessages;

public record DeleteMessagesRequest(
    DeleteMessagesDto Dto
);

public record DeleteMessagesResponse();

public class DeleteMessagesFeature(
    IDatabaseConnectionFactory connectionFactory,
    IOptions<ResQueueOptions> options
) : IDeleteMessagesFeature
{
    public async Task<OperationResult<DeleteMessagesResponse>> ExecuteAsync(DeleteMessagesRequest request)
    {
        await using var connection = connectionFactory.CreateConnection();

        await connection.OpenAsync();

        if (request.Dto.Transactional)
        {
            await using var transaction = await connection.BeginTransactionAsync();

            foreach (var message in request.Dto.Messages)
            {
                await CallRoutineAsync(message, connection);
            }

            await transaction.CommitAsync();
        }
        else
        {
            foreach (var message in request.Dto.Messages)
            {
                await CallRoutineAsync(message, connection);
            }
        }

        return OperationResult<DeleteMessagesResponse>.Success(new DeleteMessagesResponse());
    }

    private async Task CallRoutineAsync(DeleteMessagesDto.Message message, DbConnection connection)
    {
        var parameters = new DynamicParameters();
        string commandText;

        switch (options.Value.SqlEngine)
        {
            case ResQueueSqlEngine.Postgres:
                commandText = $"SELECT {options.Value.Schema}.delete_message(@message_delivery_id, @lock_id)";
                parameters.Add("message_delivery_id", message.MessageDeliveryId);
                parameters.Add("lock_id", message.LockId);
                break;

            case ResQueueSqlEngine.SqlServer:
                commandText = $"EXEC {options.Value.Schema}.DeleteMessage @messageDeliveryId, @lockId";
                parameters.Add("messageDeliveryId", message.MessageDeliveryId);
                parameters.Add("lockId", message.LockId);
                break;

            default:
                throw new NotSupportedException();
        }

        await connection.ExecuteScalarAsync(commandText, parameters);
    }
}