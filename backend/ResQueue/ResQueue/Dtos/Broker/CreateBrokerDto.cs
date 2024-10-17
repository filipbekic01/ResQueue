namespace ResQueue.Dtos;

public record CreateBrokerDto(
    string Name,
    CreatePostgresConnectionDto? PostgresConnection
);