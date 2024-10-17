namespace ResQueue;

public class DataProtectionKey
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FriendlyName { get; set; } = null!;
    public string XmlData { get; set; } = null!;
}