using Dapper;
using Marten;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ResQueue.Dtos;
using ResQueue.Models;
using ResQueue.Models.Postgres;

namespace ResQueue.Endpoints;

public static class QueueEndpoints
{
    public static void MapQueueEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder group = routes.MapGroup("queues")
            .RequireAuthorization();

        group.MapGet("view",
            async (IDocumentSession documentSession,
                UserManager<User> userManager, HttpContext httpContext,
                [FromQuery] string brokerId) =>
            {
                // Get user
                var user = await userManager.FindByEmailAsync(httpContext.User.Identity.Name);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                // Validate broker

                if (!await documentSession.Query<Broker>()
                        .Where(x => x.Id == brokerId)
                        .Where(x => x.AccessList.Any(access => access.UserId == user.Id))
                        .Where(x => x.DeletedAt == null)
                        .AnyAsync())
                {
                    return Results.Unauthorized();
                }

                await using var connection =
                    new NpgsqlConnection(
                        "host=localhost;port=5432;database=sandbox1;username=postgres;password=postgres;");

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

        group.MapGet("",
            async (IDocumentSession documentSession,
                UserManager<User> userManager, HttpContext httpContext, [FromQuery] string queueName) =>
            {
                using (var connection =
                       new NpgsqlConnection(
                           "host=localhost;port=5432;database=sandbox1;username=postgres;password=postgres;"))
                {
                    var sql = $"SELECT * FROM transport.queue WHERE name = @QueueName";
                    var queues = await connection.QueryAsync<Queue>(sql, new { QueueName = queueName });

                    return Results.Ok(queues.Select(x => new QueueDto(
                        Id: x.id,
                        Name: x.name,
                        Updated: x.updated,
                        Type: x.type,
                        AutoDelete: x.auto_delete
                    )).ToList());
                }
            });
    }
}