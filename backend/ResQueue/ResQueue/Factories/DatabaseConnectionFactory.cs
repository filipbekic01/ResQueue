using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Npgsql;
using ResQueue.Enums;

namespace ResQueue.Factories;

public class DatabaseConnectionFactory(
    IOptions<ResQueueOptions> options
) : IDatabaseConnectionFactory
{
    private readonly ResQueueOptions _options = options.Value;

    public DbConnection CreateConnection() => _options.SqlEngine switch
    {
        ResQueueSqlEngine.PostgreSql => new NpgsqlConnection(_options.ConnectionString),
        ResQueueSqlEngine.SqlServer => new SqlConnection(_options.ConnectionString),
        _ => throw new NotSupportedException($"The SQL engine '{_options.SqlEngine}' is not supported.")
    };

    public DbCommand CreateCommand(string commandText, DbConnection connection)
    {
        return connection switch
        {
            NpgsqlConnection npgsqlConnection => new NpgsqlCommand(commandText, npgsqlConnection),
            SqlConnection sqlConnection => new SqlCommand(commandText, sqlConnection),
            _ => throw new NotSupportedException("Unsupported database connection type.")
        };
    }
}