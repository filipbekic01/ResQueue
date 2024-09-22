using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace ResQueue;

public static class JsonHelper
{
    public static string ConvertBsonToJson(BsonDocument bsonDocument)
    {
        // Relaxed converts Int64 to a standard number representation in JSON
        // instead of MongoDB-specific types like NumberLong(), ensuring
        // compatibility with JSON parsers that expect standard number types.

        // This mode also normalizes other BSON types, like dates and binary data,
        // into formats that work seamlessly in typical JSON contexts.

        var jsonSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson };
        return bsonDocument.ToJson(jsonSettings);
    }
}