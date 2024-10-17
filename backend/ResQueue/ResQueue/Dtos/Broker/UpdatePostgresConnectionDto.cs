namespace ResQueue.Dtos;

public record UpdatePostgresConnectionDto(
    string Host,
    string Username,
    string Password,
    string Database,
    int Port
);