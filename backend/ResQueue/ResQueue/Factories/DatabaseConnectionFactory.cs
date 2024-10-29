using System.Data.Common;
using Microsoft.Data.SqlClient;
using Npgsql;
using ResQueue.Enums;
using ResQueue.Providers.DbConnectionProvider;

namespace ResQueue.Factories;

public class DatabaseConnectionFactory(
    IDbConnectionProvider conn
) : IDatabaseConnectionFactory
{
    public DbConnection CreateConnection() => conn.SqlEngine switch
    {
        ResQueueSqlEngine.Postgres => new NpgsqlConnection(conn.ConnectionString),
        ResQueueSqlEngine.SqlServer => new SqlConnection(conn.ConnectionString),
        _ => throw new NotSupportedException()
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