using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Options;
using ResQueue.Dtos.Messages;
using ResQueue.Enums;
using ResQueue.Factories;
using ResQueue.Providers.DbConnectionProvider;

namespace ResQueue.Features.Messages.PurgeQueue;

public record PurgeQueueRequest(
    PurgeQueueDto Dto
);

public record PurgeQueueResponse();

public class PurgeQueueFeature(
    IDatabaseConnectionFactory connectionFactory,
    IDbConnectionProvider conn
) : IPurgeQueueFeature
{
    public async Task<OperationResult<PurgeQueueResponse>> ExecuteAsync(PurgeQueueRequest request)
    {
        await using var connection = connectionFactory.CreateConnection();

        await connection.OpenAsync();

        await CallRoutineAsync(request, connection);

        return OperationResult<PurgeQueueResponse>.Success(new PurgeQueueResponse());
    }

    private async Task CallRoutineAsync(PurgeQueueRequest request, DbConnection connection)
    {
        var parameters = new DynamicParameters();
        string commandText;

        switch (conn.SqlEngine)
        {
            case ResQueueSqlEngine.Postgres:
                commandText = $"SELECT {conn.Schema}._resqueue_purge_queue_by_id(@queue_id)";
                parameters.Add("queue_id", request.Dto.QueueId);
                break;

            case ResQueueSqlEngine.SqlServer:
                commandText = $"EXEC {conn.Schema}._ResQueue_PurgeQueueById @queueId";
                parameters.Add("queueId", request.Dto.QueueId);
                break;

            default:
                throw new NotSupportedException();
        }

        await connection.ExecuteScalarAsync(commandText, parameters);
    }
}