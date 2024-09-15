using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Resqueue.Enums;

namespace Resqueue.Models;

public class BrokerAccess
{
    public ObjectId UserId { get; set; }

    [BsonRepresentation(BsonType.String)] public AccessLevel AccessLevel { get; set; }
}