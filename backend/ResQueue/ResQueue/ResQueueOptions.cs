using ResQueue.Enums;

namespace ResQueue;

public class ResQueueOptions
{
    public ResQueueSqlEngine SqlEngine { get; set; } = ResQueueSqlEngine.Postgres;
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? Database { get; set; }
    public string? Schema { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    // public bool UseTls { get; set; } = false;

    internal string ConnectionString =>
        SqlEngine switch
        {
            ResQueueSqlEngine.Postgres =>
                $"Host={Host};Port={Port ?? 5432};Database={Database};Username={Username};Password={Password};" +
                $"SearchPath={Schema};",

            ResQueueSqlEngine.SqlServer =>
                $"Server={Host},{Port ?? 1433};Database={Database};User Id={Username};Password={Password};TrustServerCertificate=True",

            _ => throw new NotSupportedException($"The SQL engine '{SqlEngine}' is not supported.")
        };
}