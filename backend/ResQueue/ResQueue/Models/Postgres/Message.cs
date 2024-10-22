namespace ResQueue.Models.Postgres;

public class Message
{
    public Guid transport_message_id { get; set; }
    public string content_type { get; set; }
    public string message_type { get; set; }
    public string body { get; set; }
    public byte[] binary_body { get; set; }
    public Guid message_id { get; set; }
    public Guid correlation_id { get; set; }
    public Guid conversation_id { get; set; }
    public Guid request_id { get; set; }
    public Guid initiator_id { get; set; }
    public Guid scheduling_token_id { get; set; }
    public string source_address { get; set; }
    public string destination_address { get; set; }
    public string response_address { get; set; }
    public string fault_address { get; set; }
    public DateTime sent_time { get; set; }
    public string headers { get; set; }
    public string host { get; set; }
}