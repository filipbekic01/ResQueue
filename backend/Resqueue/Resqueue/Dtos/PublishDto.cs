namespace Resqueue.Dtos;

public record PublishDto(
    string ExchangeId,
    string[] MessageIds
);