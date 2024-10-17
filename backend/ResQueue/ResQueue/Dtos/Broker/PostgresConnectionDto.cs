namespace ResQueue.Dtos;

public record PostgresConnectionDto(
    string Host,
    string Database,
    int Port
);