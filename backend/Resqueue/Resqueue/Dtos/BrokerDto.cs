namespace Resqueue.Dtos;

public record BrokerDto(
    string Id,
    string Name,
    int Port,
    string Url
);