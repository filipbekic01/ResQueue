using System.Text.Json.Serialization;

namespace ResQueue.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResQueueSqlEngine
{
    Postgres,
    SqlServer
}