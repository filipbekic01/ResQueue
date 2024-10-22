namespace ResQueue.Models.Postgres;

public class Queue
{
    public long id { get; set; }
    public DateTime updated { get; set; }
    public string? name { get; set; }
    public int type { get; set; }
    public bool auto_delete { get; set; }
}