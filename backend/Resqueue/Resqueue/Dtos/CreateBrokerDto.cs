namespace Resqueue.Dtos;

public record CreateBrokerDto(
    string Name,
    string Username,
    string Password,
    int Port,
    string Host,
    string VHost
);