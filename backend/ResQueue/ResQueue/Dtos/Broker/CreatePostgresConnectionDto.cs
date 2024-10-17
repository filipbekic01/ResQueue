namespace ResQueue.Dtos;

public record CreatePostgresConnectionDto(
    string Host,
    string Username,
    string Password,
    string Database,
    int Port
);