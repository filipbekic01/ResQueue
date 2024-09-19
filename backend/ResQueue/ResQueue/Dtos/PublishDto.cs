namespace ResQueue.Dtos;

public record PublishDto(
    string ExchangeId,
    string[] MessageIds
);