namespace ResQueue.Dtos.Queue;

public record QueueMetricDto(
    long QueueMetricId,
    DateTime StartTime,
    long QueueId,
    long ConsumeCount,
    long ErrorCount,
    long DeadLetterCount
);