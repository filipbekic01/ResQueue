using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Options;
using ResQueue.Dtos.Messages;
using ResQueue.Enums;
using ResQueue.Factories;
using ResQueue.Providers.DbConnectionProvider;

namespace ResQueue.Features.Messages.DeleteMessages;

public record DeleteMessagesRequest(
    DeleteMessagesDto Dto
);

public record DeleteMessagesResponse();

public class DeleteMessagesFeature(
    IDatabaseConnectionFactory connectionFactory,
    IDbConnectionProvider conn
) : IDeleteMessagesFeature
{
    public async Task<OperationResult<DeleteMessagesResponse>> ExecuteAsync(DeleteMessagesRequest request)
    {
        await using var connection = connectionFactory.CreateConnection();

        await connection.OpenAsync();

        if (request.Dto.Transactional)
        {
            await using var transaction = await connection.BeginTransactionAsync();

            foreach (var messageDeliveryId in request.Dto.MessageDeliveryIds)
            {
                await CallRoutineAsync(messageDeliveryId, connection);
            }

            await transaction.CommitAsync();
        }
        else
        {
            foreach (var messageDeliveryId in request.Dto.MessageDeliveryIds)
            {
                await CallRoutineAsync(messageDeliveryId, connection);
            }
        }

        return OperationResult<DeleteMessagesResponse>.Success(new DeleteMessagesResponse());
    }

    private async Task CallRoutineAsync(long messageDeliveryId, DbConnection connection)
    {
        var parameters = new DynamicParameters();
        string commandText;

        switch (conn.SqlEngine)
        {
            case ResQueueSqlEngine.Postgres:
                commandText = $"SELECT {conn.Schema}._resqueue_delete_message(@message_delivery_id)";
                parameters.Add("message_delivery_id", messageDeliveryId);
                break;

            case ResQueueSqlEngine.SqlServer:
                commandText = $"EXEC {conn.Schema}._ResQueue_DeleteMessage @messageDeliveryId";
                parameters.Add("messageDeliveryId", messageDeliveryId);
                break;

            default:
                throw new NotSupportedException();
        }

        await connection.ExecuteScalarAsync(commandText, parameters);
    }
}