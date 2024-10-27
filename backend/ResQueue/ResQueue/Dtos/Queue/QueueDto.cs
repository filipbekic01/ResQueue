namespace ResQueue.Dtos.Queue;

public record QueueDto(
    long Id,
    DateTime Updated,
    string Name,
    byte Type,
    int? AutoDelete
);