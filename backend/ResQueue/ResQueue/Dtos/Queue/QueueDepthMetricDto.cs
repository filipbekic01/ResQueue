namespace ResQueue.Dtos.Queue;

public record QueueDepthMetricDto(
    DateTime CapturedAt,
    long QueueId,
    long ReadyCount,
    long ErrorCount,
    long DeadLetterCount
);
