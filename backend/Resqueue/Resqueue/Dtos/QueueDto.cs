using MongoDB.Bson;

namespace Resqueue.Dtos;

public class QueueDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public BsonDocument Data { get; set; }
}