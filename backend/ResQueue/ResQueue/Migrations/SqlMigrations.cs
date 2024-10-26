using Microsoft.Extensions.Options;
using ResQueue.Enums;
using ResQueue.Factories;

namespace ResQueue.Migrations;

public class SqlMigrations(
    IDatabaseConnectionFactory connectionFactory,
    IOptions<ResQueueOptions> settings
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var cmd = GetSqlCommand(settings.Value.SqlEngine);

        await using var command = connectionFactory.CreateCommand(cmd, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private string GetSqlCommand(ResQueueSqlEngine sqlEngine) =>
        sqlEngine switch
        {
            ResQueueSqlEngine.PostgreSql => $"""
                                             CREATE OR REPLACE FUNCTION {settings.Value.Schema}._resqueue_purge_queue_by_id(queue_id bigint)
                                             RETURNS bigint
                                             AS
                                             $$
                                             BEGIN
                                                 WITH msgs AS (
                                                     DELETE FROM {settings.Value.Schema}.message_delivery md
                                                         USING (SELECT mdx.message_delivery_id
                                                                FROM {settings.Value.Schema}.message_delivery mdx
                                                                WHERE mdx.queue_id = _resqueue_purge_queue_by_id.queue_id) mds
                                                         WHERE md.message_delivery_id = mds.message_delivery_id
                                                         RETURNING md.transport_message_id)
                                                 DELETE
                                                 FROM {settings.Value.Schema}.message m
                                                     USING msgs
                                                 WHERE m.transport_message_id = msgs.transport_message_id
                                                   AND NOT EXISTS(SELECT
                                                                  FROM {settings.Value.Schema}.message_delivery md
                                                                  WHERE md.transport_message_id = m.transport_message_id);
                                             
                                                 RETURN 0;
                                             END;
                                             $$ LANGUAGE plpgsql;
                                             """,

            ResQueueSqlEngine.SqlServer => $"""
                                            CREATE OR REPLACE FUNCTION {settings.Value.Schema}._resqueue_purge_queue_by_id(queue_id bigint)
                                            RETURNS bigint
                                            AS
                                            $$
                                            BEGIN
                                                WITH msgs AS (
                                                    DELETE FROM {settings.Value.Schema}.message_delivery md
                                                        USING (SELECT mdx.message_delivery_id
                                                               FROM {settings.Value.Schema}.message_delivery mdx
                                                               WHERE mdx.queue_id = _resqueue_purge_queue_by_id.queue_id) mds
                                                        WHERE md.message_delivery_id = mds.message_delivery_id
                                                        RETURNING md.transport_message_id)
                                                DELETE
                                                FROM {settings.Value.Schema}.message m
                                                    USING msgs
                                                WHERE m.transport_message_id = msgs.transport_message_id
                                                  AND NOT EXISTS(SELECT
                                                                 FROM {settings.Value.Schema}.message_delivery md
                                                                 WHERE md.transport_message_id = m.transport_message_id);
                                            
                                                RETURN 0;
                                            END;
                                            $$ LANGUAGE plpgsql;
                                            """,

            _ => throw new ArgumentOutOfRangeException(nameof(sqlEngine), $"Unsupported SQL engine: {sqlEngine}")
        };
}