using ResQueue.Enums;

namespace ResQueue.Providers.DbConnectionProvider;

public interface IDbConnectionProvider
{
    string? Host { get; }
    int? Port { get; }
    string? Database { get; }
    string? Schema { get; }
    string? Username { get; }
    string? Password { get; }
    string? ConnectionString { get; }
    ResQueueSqlEngine SqlEngine { get; }
}