namespace ResQueue.Dtos.Queue;

public class QueueViewDto
{
    public string? QueueName { get; set; }
    public int? QueueAutoDelete { get; set; }
    public int Ready { get; set; }
    public int Scheduled { get; set; }
    public int Errored { get; set; }
    public int DeadLettered { get; set; }
    public int Locked { get; set; }
    public long ConsumeCount { get; set; }
    public long ErrorCount { get; set; }
    public long DeadLetterCount { get; set; }
    public string? CountStartTime { get; set; }
    public int CountDuration { get; set; }
}