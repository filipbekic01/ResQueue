namespace ResQueue.Dtos;

public class QueueDto
{
    public string Id { get; set; } = null!;
    public string RawData { get; set; } = null!;
    public int TotalMessages { get; set; }
    public int Messages { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime CreatedAt { get; set; }
}