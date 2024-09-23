using Microsoft.AspNetCore.DataProtection.Repositories;
using MongoDB.Driver;
using System.Xml.Linq;

namespace ResQueue;

public class MongoDbXmlRepository : IXmlRepository
{
    private readonly IMongoCollection<DataProtectionKey> _collection;

    public MongoDbXmlRepository(IMongoClient mongoClient, string databaseName)
    {
        var database = mongoClient.GetDatabase(databaseName);
        _collection = database.GetCollection<DataProtectionKey>("data_protection_keys");
    }

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        var keys = _collection.Find(FilterDefinition<DataProtectionKey>.Empty).ToList();

        return keys.Select(k => XElement.Parse(k.XmlData)).ToList().AsReadOnly();
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        var key = new DataProtectionKey
        {
            FriendlyName = friendlyName,
            XmlData = element.ToString(SaveOptions.DisableFormatting)
        };

        _collection.InsertOne(key);
    }
}