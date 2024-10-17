namespace ResQueue.Models;

public class Exchange
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string BrokerId { get; set; }
    public string RawData { get; set; } = null!;
}