using System.Data;
using Dapper;
using ResQueue.Enums;

namespace ResQueue.QueueDepth;

/// <summary>
/// SQL for the ResQueue-owned queue depth history table.
/// Postgres: {schema}._resqueue_queue_depth_metric, SQL Server: {schema}._ResQueue_QueueDepthMetric.
/// All timestamps are UTC (timestamptz on Postgres, DATETIME2 holding UTC on SQL Server).
/// </summary>
internal static class QueueDepthHistorySql
{
    /// <summary>
    /// Adds a UTC timestamp parameter typed correctly for the engine: timestamptz on Postgres
    /// (Npgsql requires DateTimeKind.Utc) and datetime2 on SQL Server (avoids the legacy datetime type).
    /// </summary>
    public static void AddUtcParameter(DynamicParameters parameters, ResQueueSqlEngine engine, string name,
        DateTime utcValue)
    {
        var value = DateTime.SpecifyKind(utcValue, DateTimeKind.Utc);
        parameters.Add(name, value, engine == ResQueueSqlEngine.SqlServer ? DbType.DateTime2 : null);
    }

    /// <summary>
    /// Idempotent DDL that creates the table and its indexes if they don't exist yet.
    /// </summary>
    public static string CreateTable(ResQueueSqlEngine engine, string? schema) => engine switch
    {
        ResQueueSqlEngine.Postgres => $"""
                                       CREATE TABLE IF NOT EXISTS {schema}._resqueue_queue_depth_metric (
                                           id                bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                                           captured_at       timestamptz NOT NULL,
                                           queue_id          bigint      NOT NULL,
                                           ready_count       bigint      NOT NULL,
                                           error_count       bigint      NOT NULL,
                                           dead_letter_count bigint      NOT NULL
                                       );
                                       CREATE INDEX IF NOT EXISTS ix_resqueue_queue_depth_metric_queue_id_captured_at
                                           ON {schema}._resqueue_queue_depth_metric (queue_id, captured_at);
                                       CREATE INDEX IF NOT EXISTS ix_resqueue_queue_depth_metric_captured_at
                                           ON {schema}._resqueue_queue_depth_metric (captured_at);
                                       """,
        ResQueueSqlEngine.SqlServer => $"""
                                        IF OBJECT_ID(N'{schema}._ResQueue_QueueDepthMetric', N'U') IS NULL
                                        BEGIN
                                            CREATE TABLE {schema}._ResQueue_QueueDepthMetric (
                                                Id              BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                                CapturedAt      DATETIME2 NOT NULL,
                                                QueueId         BIGINT    NOT NULL,
                                                ReadyCount      BIGINT    NOT NULL,
                                                ErrorCount      BIGINT    NOT NULL,
                                                DeadLetterCount BIGINT    NOT NULL
                                            );
                                        END;

                                        IF NOT EXISTS (SELECT 1 FROM sys.indexes
                                                       WHERE name = N'IX_ResQueue_QueueDepthMetric_QueueId_CapturedAt'
                                                         AND object_id = OBJECT_ID(N'{schema}._ResQueue_QueueDepthMetric'))
                                        BEGIN
                                            CREATE INDEX IX_ResQueue_QueueDepthMetric_QueueId_CapturedAt
                                                ON {schema}._ResQueue_QueueDepthMetric (QueueId, CapturedAt);
                                        END;

                                        IF NOT EXISTS (SELECT 1 FROM sys.indexes
                                                       WHERE name = N'IX_ResQueue_QueueDepthMetric_CapturedAt'
                                                         AND object_id = OBJECT_ID(N'{schema}._ResQueue_QueueDepthMetric'))
                                        BEGIN
                                            CREATE INDEX IX_ResQueue_QueueDepthMetric_CapturedAt
                                                ON {schema}._ResQueue_QueueDepthMetric (CapturedAt);
                                        END;
                                        """,
        _ => throw new NotSupportedException("Unsupported SQL engine")
    };

    /// <summary>
    /// Snapshots ready/error/dead-letter counts for every queue from MassTransit's queues view, keyed by the
    /// primary (type = 1) queue id. Skips queues that already have a sample newer than @DedupeSince so that
    /// several app instances running the sampler don't write duplicate rows.
    /// Parameters: @CapturedAt, @DedupeSince (UTC).
    /// </summary>
    public static string InsertSample(ResQueueSqlEngine engine, string? schema) => engine switch
    {
        ResQueueSqlEngine.Postgres => $"""
                                       INSERT INTO {schema}._resqueue_queue_depth_metric
                                           (captured_at, queue_id, ready_count, error_count, dead_letter_count)
                                       SELECT @CapturedAt, q.id, qs.ready, qs.errored, qs.dead_lettered
                                       FROM {schema}.queues qs
                                                INNER JOIN {schema}.queue q ON q.name = qs.queue_name AND q.type = 1
                                       WHERE NOT EXISTS (SELECT 1
                                                         FROM {schema}._resqueue_queue_depth_metric m
                                                         WHERE m.queue_id = q.id
                                                           AND m.captured_at > @DedupeSince);
                                       """,
        ResQueueSqlEngine.SqlServer => $"""
                                        INSERT INTO {schema}._ResQueue_QueueDepthMetric
                                            (CapturedAt, QueueId, ReadyCount, ErrorCount, DeadLetterCount)
                                        SELECT @CapturedAt, q.Id, qs.Ready, qs.Errored, qs.DeadLettered
                                        FROM {schema}.Queues qs
                                                 INNER JOIN {schema}.Queue q ON q.Name = qs.QueueName AND q.Type = 1
                                        WHERE NOT EXISTS (SELECT 1
                                                          FROM {schema}._ResQueue_QueueDepthMetric m
                                                          WHERE m.QueueId = q.Id
                                                            AND m.CapturedAt > @DedupeSince);
                                        """,
        _ => throw new NotSupportedException("Unsupported SQL engine")
    };

    /// <summary>
    /// Deletes samples older than @Cutoff (UTC).
    /// </summary>
    public static string DeleteExpired(ResQueueSqlEngine engine, string? schema) => engine switch
    {
        ResQueueSqlEngine.Postgres => $"""
                                       DELETE FROM {schema}._resqueue_queue_depth_metric
                                       WHERE captured_at < @Cutoff;
                                       """,
        ResQueueSqlEngine.SqlServer => $"""
                                        DELETE FROM {schema}._ResQueue_QueueDepthMetric
                                        WHERE CapturedAt < @Cutoff;
                                        """,
        _ => throw new NotSupportedException("Unsupported SQL engine")
    };

    /// <summary>
    /// Reads samples for one queue captured at or after @Since (UTC), oldest first.
    /// Parameters: @QueueId, @Since.
    /// </summary>
    public static string SelectForQueue(ResQueueSqlEngine engine, string? schema) => engine switch
    {
        ResQueueSqlEngine.Postgres => $"""
                                       SELECT
                                           captured_at AS CapturedAt,
                                           queue_id AS QueueId,
                                           ready_count AS ReadyCount,
                                           error_count AS ErrorCount,
                                           dead_letter_count AS DeadLetterCount
                                       FROM {schema}._resqueue_queue_depth_metric
                                       WHERE queue_id = @QueueId
                                         AND captured_at >= @Since
                                       ORDER BY captured_at;
                                       """,
        ResQueueSqlEngine.SqlServer => $"""
                                        SELECT
                                            CapturedAt,
                                            QueueId,
                                            ReadyCount,
                                            ErrorCount,
                                            DeadLetterCount
                                        FROM {schema}._ResQueue_QueueDepthMetric
                                        WHERE QueueId = @QueueId
                                          AND CapturedAt >= @Since
                                        ORDER BY CapturedAt;
                                        """,
        _ => throw new NotSupportedException("Unsupported SQL engine")
    };
}
