using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebOne.Models.MongoDB;

public class Broker
{
    [BsonId] public ObjectId Id { get; set; }
    public string Name { get; set; }
    public string UserId { get; set; }
    public string Auth { get; set; }
    public int Port { get; set; }
    public string Url { get; set; }
}