using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ResQueue.Dtos.Messages;
using ResQueue.Dtos.Queue;
using ResQueue.Enums;
using ResQueue.Factories;
using ResQueue.Features.Messages.PurgeQueue;
using ResQueue.Providers.DbConnectionProvider;

namespace ResQueue.Endpoints;

public static class QueuesEndpoints
{
    public static void MapQueueEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("queues");

        group.MapGet("view", async (IDatabaseConnectionFactory connectionFactory, IDbConnectionProvider conn) =>
        {
            var sql = conn.SqlEngine switch
            {
                ResQueueSqlEngine.Postgres => $"""
                                               SELECT 
                                                   queue_name AS QueueName,
                                                   queue_auto_delete AS QueueAutoDelete,
                                                   ready AS Ready,
                                                   scheduled AS Scheduled,
                                                   errored AS Errored,
                                                   dead_lettered AS DeadLettered,
                                                   locked AS Locked,
                                                   consume_count AS ConsumeCount,
                                                   error_count AS ErrorCount,
                                                   dead_letter_count AS DeadLetterCount,
                                                   '' AS CountStartTime,
                                                   count_duration AS CountDuration,
                                                   queue_max_delivery_count AS QueueMaxDeliveryCount
                                               FROM {conn.Schema}.queues;
                                               """,
                ResQueueSqlEngine.SqlServer => $"""
                                                SELECT 
                                                    QueueName,
                                                    QueueAutoDelete,
                                                    Ready,
                                                    Scheduled,
                                                    Errored,
                                                    DeadLettered,
                                                    Locked,
                                                    ConsumeCount,
                                                    ErrorCount,
                                                    DeadLetterCount,
                                                    CountStartTime,
                                                    CountDuration,
                                                    QueueMaxDeliveryCount
                                                FROM {conn.Schema}.Queues;
                                                """,
                _ => throw new NotSupportedException("Unsupported SQL engine")
            };

            await using var connection = connectionFactory.CreateConnection();

            var queuesFromView = await connection.QueryAsync<QueueViewDto>(sql);

            return Results.Ok(queuesFromView);
        });

        group.MapGet("view/{queueName}", async (IDatabaseConnectionFactory connectionFactory,
            IDbConnectionProvider conn, string queueName) =>
        {
            var sql = conn.SqlEngine switch
            {
                ResQueueSqlEngine.Postgres => $"""
                                               SELECT 
                                                   queue_name AS QueueName,
                                                   queue_auto_delete AS QueueAutoDelete,
                                                   ready AS Ready,
                                                   scheduled AS Scheduled,
                                                   errored AS Errored,
                                                   dead_lettered AS DeadLettered,
                                                   locked AS Locked,
                                                   consume_count AS ConsumeCount,
                                                   error_count AS ErrorCount,
                                                   dead_letter_count AS DeadLetterCount,
                                                   '' AS CountStartTime,
                                                   count_duration AS CountDuration,
                                                   queue_max_delivery_count AS QueueMaxDeliveryCount
                                               FROM {conn.Schema}.queues
                                               WHERE queue_name = @QueueName;
                                               """,
                ResQueueSqlEngine.SqlServer => $"""
                                                SELECT 
                                                    QueueName,
                                                    QueueAutoDelete,
                                                    Ready,
                                                    Scheduled,
                                                    Errored,
                                                    DeadLettered,
                                                    Locked,
                                                    ConsumeCount,
                                                    ErrorCount,
                                                    DeadLetterCount,
                                                    CountStartTime,
                                                    CountDuration,
                                                    QueueMaxDeliveryCount
                                                FROM {conn.Schema}.Queues
                                                WHERE QueueName = @QueueName;
                                                """,
                _ => throw new NotSupportedException("Unsupported SQL engine")
            };

            await using var connection = connectionFactory.CreateConnection();

            var queueView = await connection.QuerySingleAsync<QueueViewDto>(sql, new { QueueName = queueName });

            return Results.Ok(queueView);
        });

        group.MapGet("",
            async ([FromQuery] string queueName, IDatabaseConnectionFactory connectionFactory,
                IDbConnectionProvider conn) =>
            {
                var sql = conn.SqlEngine switch
                {
                    ResQueueSqlEngine.Postgres => $"""
                                                   SELECT 
                                                       id AS Id,
                                                       updated AS Updated,
                                                       name AS Name,
                                                       type AS Type,
                                                       auto_delete AS AutoDelete
                                                   FROM {conn.Schema}.queue
                                                   WHERE name = @QueueName;
                                                   """,
                    ResQueueSqlEngine.SqlServer => $"""
                                                    SELECT 
                                                        Id,
                                                        Updated,
                                                        Name,
                                                        CAST(type AS INT) AS Type,
                                                        AutoDelete
                                                    FROM {conn.Schema}.Queue
                                                    WHERE Name = @QueueName;
                                                    """,
                    _ => throw new NotSupportedException("Unsupported SQL engine")
                };

                await using var connection = connectionFactory.CreateConnection();

                var queues = await connection.QueryAsync<QueueDto>(sql, new { QueueName = queueName });

                return Results.Ok(queues);
            });

        group.MapPost("purge",
            async (IPurgeQueueFeature feature, IDbConnectionProvider conn, [FromBody] PurgeQueueDto dto) =>
            {
                var result = await feature.ExecuteAsync(new PurgeQueueRequest(dto));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem!);
            });
    }
}