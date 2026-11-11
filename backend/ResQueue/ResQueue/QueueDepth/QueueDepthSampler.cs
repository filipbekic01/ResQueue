using Dapper;
using Microsoft.Extensions.Options;
using ResQueue.Factories;
using ResQueue.Providers.DbConnectionProvider;

namespace ResQueue.QueueDepth;

/// <summary>
/// Periodically samples ready/error/dead-letter counts for every queue and stores them in the
/// ResQueue queue depth history table, deleting samples older than the configured retention.
/// Never throws out of <see cref="ExecuteAsync"/>: database errors are logged and retried on the next tick.
/// </summary>
public class QueueDepthSampler(
    IDatabaseConnectionFactory connectionFactory,
    IDbConnectionProvider conn,
    IOptions<ResQueueOptions> options,
    IHostApplicationLifetime lifetime,
    ILogger<QueueDepthSampler> logger
) : BackgroundService
{
    private bool _tableEnsured;
    private int _consecutiveFailures;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var historyOptions = options.Value.QueueDepthHistory;
        if (!historyOptions.Enabled)
        {
            return;
        }

        try
        {
            // Wait until all hosted services (including MassTransit and ResQueue migrations) have started,
            // so the transport tables/views exist and we don't race their DDL.
            await WaitForApplicationStartedAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        var interval = historyOptions.EffectiveSampleInterval;
        var retention = historyOptions.EffectiveRetention;

        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                await SampleAsync(interval, retention, stoppingToken);

                if (_consecutiveFailures > 0)
                {
                    logger.LogInformation("ResQueue queue depth sampling recovered after {Failures} failed attempt(s)",
                        _consecutiveFailures);
                }

                _consecutiveFailures = 0;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // Table may have been dropped, DB may be unavailable, transport schema may not exist yet...
                // Re-check the table on the next attempt and keep going.
                _tableEnsured = false;
                _consecutiveFailures++;

                // Avoid flooding logs while the database is down: log the first failure and then every 10th.
                if (_consecutiveFailures == 1 || _consecutiveFailures % 10 == 0)
                {
                    logger.LogWarning(ex,
                        "ResQueue queue depth sampling failed ({Failures} consecutive failure(s)); will retry in {Interval}",
                        _consecutiveFailures, interval);
                }
            }

            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        } while (!stoppingToken.IsCancellationRequested);
    }

    private async Task SampleAsync(TimeSpan interval, TimeSpan retention, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (!_tableEnsured)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                QueueDepthHistorySql.CreateTable(conn.SqlEngine, conn.Schema),
                cancellationToken: cancellationToken));
            _tableEnsured = true;
        }

        var now = DateTime.UtcNow;

        var insertParameters = new DynamicParameters();
        QueueDepthHistorySql.AddUtcParameter(insertParameters, conn.SqlEngine, "CapturedAt", now);
        // Half an interval: tolerates timer jitter while preventing duplicates from multiple app instances.
        QueueDepthHistorySql.AddUtcParameter(insertParameters, conn.SqlEngine, "DedupeSince", now - interval / 2);

        await connection.ExecuteAsync(new CommandDefinition(
            QueueDepthHistorySql.InsertSample(conn.SqlEngine, conn.Schema),
            insertParameters,
            cancellationToken: cancellationToken));

        var deleteParameters = new DynamicParameters();
        QueueDepthHistorySql.AddUtcParameter(deleteParameters, conn.SqlEngine, "Cutoff", now - retention);

        await connection.ExecuteAsync(new CommandDefinition(
            QueueDepthHistorySql.DeleteExpired(conn.SqlEngine, conn.Schema),
            deleteParameters,
            cancellationToken: cancellationToken));
    }

    private async Task WaitForApplicationStartedAsync(CancellationToken stoppingToken)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var startedRegistration = lifetime.ApplicationStarted.Register(() => tcs.TrySetResult());
        await using var stoppingRegistration = stoppingToken.Register(() => tcs.TrySetCanceled(stoppingToken));
        await tcs.Task;
    }
}
