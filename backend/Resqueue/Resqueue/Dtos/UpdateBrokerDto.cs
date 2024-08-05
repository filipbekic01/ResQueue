namespace WebOne.Dtos;

public record UpdateBrokerDto(
    string Username,
    string Password,
    int Port,
    string Url
);