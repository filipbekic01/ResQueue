namespace ResQueue.Dtos.Messages;

public record SubscriptionDto(
    string TopicName,
    string DestinationType,
    string DestinationName,
    int SubscriptionType,
    string RoutingKey
);