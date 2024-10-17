namespace ResQueue.Dtos;

public class QueueDto
{
    public string QueueName { get; set; }
    public bool QueueAutoDelete { get; set; }
    public long Ready { get; set; }
    public long Scheduled { get; set; }
    public long Errored { get; set; }
    public long DeadLettered { get; set; }
    public long Locked { get; set; }
    public long ConsumeCount { get; set; }
    public long ErrorCount { get; set; }
    public long DeadLetterCount { get; set; }
    public long CountDuration { get; set; }
}