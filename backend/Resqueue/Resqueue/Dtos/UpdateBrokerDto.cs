namespace Resqueue.Dtos;

public record UpdateBrokerDto(
    string Username,
    string Password,
    int Port,
    string Url
);