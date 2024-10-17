namespace ResQueue.Models;

public class Queue
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string BrokerId { get; set; }
    public int TotalMessages { get; set; }
    public int Messages { get; set; }
    public string RawData { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public long NextMessageOrder { get; set; }
}