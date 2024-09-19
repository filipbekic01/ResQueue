namespace ResQueue.Dtos.Broker;

public record CreateBrokerInvitationDto(
    string BrokerId,
    string Email
);