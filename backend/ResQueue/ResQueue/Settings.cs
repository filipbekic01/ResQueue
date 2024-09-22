namespace ResQueue;

public class Settings
{
    public string SmtpHost { get; set; } = null!;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = null!;
    public string SmtpPassword { get; set; } = null!;

    public string MongoDBConnectionString { get; set; } = null!;

    public string StripeSecret { get; set; } = null!;
    public string StripeSecretWebhook { get; set; } = null!;
    public string WebsiteUrl { get; set; } = null!;

    public string StripeEssentialsPriceId { get; set; } = null!;
    public string StripeUltimatePriceId { get; set; } = null!;
}