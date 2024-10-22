using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Npgsql;
using ResQueue.Dtos;
using ResQueue.Dtos.Queue;
using ResQueue.Models.Postgres;

namespace ResQueue.Endpoints;

public static class QueuesEndpoints
{
    public static void MapQueueEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("queues");

        group.MapGet("view", async (IOptions<Settings> settings) =>
        {
            await using var connection = new NpgsqlConnection(settings.Value.PostgreSQLConnectionString);

            var queuesFromView = await connection.QueryAsync<QueueView>($"SELECT * FROM transport.queues;");

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

        group.MapGet("view/{queueName}",
            async (IOptions<Settings> settings, string queueName) =>
            {
                await using var connection = new NpgsqlConnection(settings.Value.PostgreSQLConnectionString);

                var queueView =
                    await connection.QuerySingleAsync<QueueView>(
                        $"SELECT * FROM transport.queues WHERE queue_name = @QueueName;", new
                        {
                            QueueName = queueName
                        });

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

        group.MapGet("", async ([FromQuery] string queueName, IOptions<Settings> settings) =>
        {
            await using var connection = new NpgsqlConnection(settings.Value.PostgreSQLConnectionString);

            var sql = $"SELECT * FROM transport.queue WHERE name = @QueueName";
            var queues = await connection.QueryAsync<Queue>(sql, new { QueueName = queueName });

            return Results.Ok(queues.Select(x => new QueueDto(
                Id: x.id,
                Name: x.name,
                Updated: x.updated,
                Type: x.type,
                AutoDelete: x.auto_delete
            )).ToList());
        });
    }
}