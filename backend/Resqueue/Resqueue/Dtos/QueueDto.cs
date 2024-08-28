namespace Resqueue.Dtos;

public class QueueDto
{
    public string Id { get; set; }
    public string RawData { get; set; }
    public long TotalMessages { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime CreatedAt { get; set; }
}