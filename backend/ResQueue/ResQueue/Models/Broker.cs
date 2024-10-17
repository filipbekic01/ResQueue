namespace ResQueue.Models;

public class Broker
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CreatedByUserId { get; set; }
    public List<BrokerAccess> AccessList { get; set; } = [];
    public string System { get; set; } = null!;
    public string Name { get; set; } = null!;
    public PostgresConnection? PostgresConnection { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? SyncedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}