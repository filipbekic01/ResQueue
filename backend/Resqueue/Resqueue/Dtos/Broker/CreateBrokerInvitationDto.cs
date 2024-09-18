namespace Resqueue.Dtos;

public record CreateBrokerInvitationDto(
    string BrokerId,
    string Email
);