using MongoDB.Bson;

namespace ResQueue;

public class DataProtectionKey
{
    public ObjectId Id { get; set; }
    public string FriendlyName { get; set; }
    public string XmlData { get; set; }
}