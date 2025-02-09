namespace ResQueue.Dtos.Queue;

public record QueueMetricDto(
    long QueueMetricId,
    DateTime StartTime,
    TimeSpan Duration,
    long QueueId,
    long ConsumeCount,
    long ErrorCount,
    long DeadLetterCount
);