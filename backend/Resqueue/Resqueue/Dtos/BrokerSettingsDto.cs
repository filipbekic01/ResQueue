namespace Resqueue.Dtos;

public record BrokerSettingsDto(
    List<string> QuickSearches,
    string DeadLetterQueueSuffix,
    string MessageFormat,
    string MessageStructure
);