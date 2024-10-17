namespace ResQueue.Dtos;

public class UserBasicDto
{
    public string Id { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Avatar { get; set; } = null!;
    public string? SubscriptionType { get; set; }
}