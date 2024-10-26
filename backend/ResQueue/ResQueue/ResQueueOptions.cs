using ResQueue.Enums;

namespace ResQueue;

public class ResQueueOptions
{
    public ResQueueSqlEngine SqlEngine { get; set; } = ResQueueSqlEngine.PostgreSql;
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? Database { get; set; }
    public string? Schema { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }

    internal string ConnectionString =>
        SqlEngine switch
        {
            ResQueueSqlEngine.PostgreSql =>
                $"Host={Host};Port={Port ?? 5432};Database={Database};Username={Username};Password={Password}",

            ResQueueSqlEngine.SqlServer =>
                $"Server={Host},{Port ?? 1433};Database={Database};User Id={Username};Password={Password};",

            _ => throw new NotSupportedException($"The SQL engine '{SqlEngine}' is not supported.")
        };
}