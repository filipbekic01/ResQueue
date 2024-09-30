using MongoDB.Bson;

namespace ResQueue;

public class DataProtectionKey
{
    public ObjectId Id { get; set; }
    public string FriendlyName { get; set; } = null!;
    public string XmlData { get; set; } = null!;
}