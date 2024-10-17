namespace ResQueue.Dtos;

public record BrokerSettingsDto(
    List<string> QuickSearches,
    string DeadLetterQueueSuffix,
    string MessageFormat,
    string MessageStructure,
    string QueueTrimPrefix,
    string? DefaultQueueSortField,
    int DefaultQueueSortOrder,
    string DefaultQueueSearch
);