namespace Resqueue.Dtos;

public class MessageDto
{
    public string Id { get; set; }
    public string RawData { get; set; }
    public string Summary { get; set; }
    public bool IsReviewed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}