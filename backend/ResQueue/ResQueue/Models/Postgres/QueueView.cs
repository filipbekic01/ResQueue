namespace ResQueue.Models.Postgres;

public class QueueView
{
    public string queue_name { get; set; }
    public bool queue_auto_delete { get; set; }
    public long ready { get; set; }
    public long scheduled { get; set; }
    public long errored { get; set; }
    public long dead_lettered { get; set; }
    public long locked { get; set; }
    public long consume_count { get; set; }
    public long error_count { get; set; }
    public long dead_letter_count { get; set; }
    public long count_duration { get; set; }
}