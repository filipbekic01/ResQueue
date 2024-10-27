using ResQueue.Enums;

namespace ResQueue.Dtos;

public record AuthDto(
    ResQueueSqlEngine SqlEngine,
    string? Username,
    string? Database,
    string? Schema,
    int? Port
);