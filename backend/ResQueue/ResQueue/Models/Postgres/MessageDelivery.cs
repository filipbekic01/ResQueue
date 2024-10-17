namespace ResQueue.Models.Postgres;

public class MessageDelivery
{
    public long message_delivery_id { get; set; }
    public Guid transport_message_id { get; set; }
    public long queue_id { get; set; }
    public short priority { get; set; }
    public DateTime enqueue_time { get; set; }
    public DateTime expiration_time { get; set; }
    public string partition_key { get; set; }
    public string routing_key { get; set; }
    public Guid consumer_id { get; set; }
    public Guid lock_id { get; set; }
    public int delivery_count { get; set; }
    public int max_delivery_count { get; set; }
    public DateTime last_delivered { get; set; }
    public string transport_headers { get; set; }

    public Message message { get; set; }
}