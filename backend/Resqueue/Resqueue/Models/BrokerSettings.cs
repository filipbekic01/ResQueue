namespace Resqueue.Models;

public class BrokerSettings
{
    public List<string> QuickSearches { get; set; } = ["error", "fail", "dead"];
    public string DeadLetterQueueSuffix { get; set; } = "_error";
    public string MessageFormat { get; set; } = "clean";
    public string MessageStructure { get; set; } = "both";
}