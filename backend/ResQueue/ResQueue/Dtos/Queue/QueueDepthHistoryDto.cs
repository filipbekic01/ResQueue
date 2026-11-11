namespace ResQueue.Dtos.Queue;

public record QueueDepthHistoryDto(
    bool Enabled,
    int SampleIntervalSeconds,
    IEnumerable<QueueDepthMetricDto> Items
);
