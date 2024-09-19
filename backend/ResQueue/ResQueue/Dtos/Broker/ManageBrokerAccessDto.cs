using System.Text.Json.Serialization;
using ResQueue.Enums;

namespace ResQueue.Dtos.Broker;

public class ManageBrokerAccessDto
{
    public string BrokerId { get; set; } = null!;
    public string UserId { get; set; } = null!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AccessLevel? AccessLevel { get; set; }
}