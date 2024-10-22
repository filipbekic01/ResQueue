namespace ResQueue.Dtos;

public record QueueDto(
    long Id,
    DateTime Updated,
    string Name,
    int Type,
    bool AutoDelete
);