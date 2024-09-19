using System.Text.Json.Serialization;
using Resqueue.Enums;

namespace Resqueue.Dtos.Broker;

public class BrokerAccessDto
{
    public string UserId { get; set; } = null!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AccessLevel AccessLevel { get; set; }
}