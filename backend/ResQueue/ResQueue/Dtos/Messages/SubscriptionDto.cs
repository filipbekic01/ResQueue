namespace ResQueue.Dtos.Messages;

public record SubscriptionDto(
    string TopicName,
    string DestinationType,
    string DestinationName,
    byte SubscriptionType,
    string RoutingKey
);
