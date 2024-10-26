using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ResQueue.Dtos.Messages;
using ResQueue.Dtos.Queue;
using ResQueue.Enums;
using ResQueue.Factories;
using ResQueue.Features.Messages.PurgeQueue;
using ResQueue.Models.Postgres;

namespace ResQueue.Endpoints;

public static class QueuesEndpoints
{
    public static void MapQueueEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("queues");

        group.MapGet("view", async (IDatabaseConnectionFactory connectionFactory, IOptions<ResQueueOptions> options) =>
        {
            var sql = options.Value.SqlEngine switch
            {
                ResQueueSqlEngine.PostgreSql => "SELECT * FROM transport.queues;",
                ResQueueSqlEngine.SqlServer => "SELECT * FROM transport.queues",
                _ => throw new NotSupportedException("Unsupported SQL engine")
            };

            await using var connection = connectionFactory.CreateConnection();

            var queuesFromView = await connection.QueryAsync<QueueView>(sql);

            return Results.Ok(queuesFromView.Select(x => new QueueViewDto()
            {
                QueueName = x.queue_name,
                QueueAutoDelete = x.queue_auto_delete,
                Ready = x.ready,
                Scheduled = x.scheduled,
                Errored = x.errored,
                DeadLettered = x.dead_lettered,
                Locked = x.locked,
                ConsumeCount = x.consume_count,
                ErrorCount = x.error_count,
                DeadLetterCount = x.dead_letter_count,
                CountDuration = x.count_duration
            }).ToList());
        });

        group.MapGet("view/{queueName}", async (IDatabaseConnectionFactory connectionFactory,
            IOptions<ResQueueOptions> options, string queueName) =>
        {
            var sql = options.Value.SqlEngine switch
            {
                ResQueueSqlEngine.PostgreSql => "SELECT * FROM transport.queues WHERE queue_name = @QueueName;",
                ResQueueSqlEngine.SqlServer =>
                    "SELECT * FROM transport.queues WHERE queue_name = @QueueName",
                _ => throw new NotSupportedException("Unsupported SQL engine")
            };

            await using var connection = connectionFactory.CreateConnection();

            var queueView = await connection.QuerySingleAsync<QueueView>(sql, new { QueueName = queueName });

            return Results.Ok(new QueueViewDto()
            {
                QueueName = queueView.queue_name,
                QueueAutoDelete = queueView.queue_auto_delete,
                Ready = queueView.ready,
                Scheduled = queueView.scheduled,
                Errored = queueView.errored,
                DeadLettered = queueView.dead_lettered,
                Locked = queueView.locked,
                ConsumeCount = queueView.consume_count,
                ErrorCount = queueView.error_count,
                DeadLetterCount = queueView.dead_letter_count,
                CountDuration = queueView.count_duration
            });
        });

        group.MapGet("",
            async ([FromQuery] string queueName, IDatabaseConnectionFactory connectionFactory,
                IOptions<ResQueueOptions> options) =>
            {
                var sql = options.Value.SqlEngine switch
                {
                    ResQueueSqlEngine.PostgreSql => "SELECT * FROM transport.queue WHERE name = @QueueName;",
                    ResQueueSqlEngine.SqlServer => "SELECT * FROM transport.queue WHERE name = @QueueName;",
                    _ => throw new NotSupportedException("Unsupported SQL engine")
                };

                await using var connection = connectionFactory.CreateConnection();

                var queues = await connection.QueryAsync<Queue>(sql, new { QueueName = queueName });

                return Results.Ok(queues.Select(x => new QueueDto(
                    Id: x.id,
                    Name: x.name,
                    Updated: x.updated,
                    Type: x.type,
                    AutoDelete: x.auto_delete
                )).ToList());
            });

        group.MapPost("purge",
            async (IPurgeQueueFeature feature, IOptions<ResQueueOptions> options, [FromBody] PurgeQueueDto dto) =>
            {
                var result = await feature.ExecuteAsync(new PurgeQueueRequest(dto));

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Problem(result.Problem!);
            });
    }
}