namespace Resqueue;

public class Settings
{
    public string SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; }
    public string SmtpPassword { get; set; }
    
    public string MongoDBConnectionString { get; set; }
}