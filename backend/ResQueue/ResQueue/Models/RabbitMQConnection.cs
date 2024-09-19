namespace ResQueue.Models;

public class RabbitMQConnection
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required int ManagementPort { get; set; }
    public required bool ManagementTls { get; set; }
    public required int AmqpPort { get; set; }
    public required bool AmqpTls { get; set; }
    public required string Host { get; set; }
    public required string VHost { get; set; }
}