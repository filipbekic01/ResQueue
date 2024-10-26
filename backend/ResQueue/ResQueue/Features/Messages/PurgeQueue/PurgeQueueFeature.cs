using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Options;
using ResQueue.Dtos.Messages;
using ResQueue.Enums;
using ResQueue.Factories;

namespace ResQueue.Features.Messages.PurgeQueue;

public record PurgeQueueRequest(
    PurgeQueueDto Dto
);

public record PurgeQueueResponse();

public class PurgeQueueFeature(
    IDatabaseConnectionFactory connectionFactory,
    IOptions<ResQueueOptions> settings
) : IPurgeQueueFeature
{
    public async Task<OperationResult<PurgeQueueResponse>> ExecuteAsync(PurgeQueueRequest request)
    {
        await using var connection = connectionFactory.CreateConnection();

        await connection.OpenAsync();

        await CallRoutineAsync(request, connection);

        return OperationResult<PurgeQueueResponse>.Success(new PurgeQueueResponse());
    }

    private async Task<int?> CallRoutineAsync(PurgeQueueRequest request, DbConnection connection)
    {
        var parameters = new DynamicParameters();
        string commandText;

        switch (settings.Value.SqlEngine)
        {
            case ResQueueSqlEngine.PostgreSql:
                commandText = "SELECT transport._resqueue_purge_queue_by_id(@queue_id)";
                parameters.Add("queue_id", request.Dto.QueueId);
                break;

            case ResQueueSqlEngine.SqlServer:
                commandText = "EXEC transport._resqueue_purge_queue_by_id @queue_id";
                parameters.Add("queue_id", request.Dto.QueueId);
                break;

            default:
                throw new NotSupportedException();
        }

        return await connection.QuerySingleAsync<int?>(commandText, parameters);
    }
}