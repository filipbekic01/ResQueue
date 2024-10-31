using System.Data.Common;
using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Npgsql;
using ResQueue.Enums;

namespace ResQueue.Providers.DbConnectionProvider
{
    public class DbConnectionProvider : IDbConnectionProvider
    {
        public string? Host { get; private set; }
        public int? Port { get; private set; }
        public string? Database { get; private set; }
        public string? Schema { get; private set; }
        public string? Username { get; private set; }
        public string? Password { get; private set; }
        public string? ConnectionString { get; private set; }
        public ResQueueSqlEngine SqlEngine { get; private set; }

        public DbConnectionProvider(IOptions<SqlTransportOptions> options, IOptions<ResQueueOptions> resQueueOptions)
        {
            SqlEngine = resQueueOptions.Value.SqlEngine;
            Host = options.Value.Host;
            Port = options.Value.Port;
            Database = options.Value.Database;
            Schema = string.IsNullOrEmpty(options.Value.Schema)
                ? "transport"
                : options.Value.Schema;
            Username = options.Value.Username;
            Password = options.Value.Password;
            ConnectionString = options.Value.ConnectionString;

            if (!string.IsNullOrEmpty(options.Value.ConnectionString))
            {
                ParseConnectionString(options.Value.ConnectionString);
            }
            else
            {
                ConnectionString = GenerateConnectionString();
            }
        }

        private void ParseConnectionString(string connectionString)
        {
            DbConnectionStringBuilder builder;

            switch (SqlEngine)
            {
                case ResQueueSqlEngine.SqlServer:
                    builder = new SqlConnectionStringBuilder(connectionString);
                    Host = builder.TryGetValue("Data Source", out var sqlHost) ? sqlHost.ToString() : null;
                    Database = builder.TryGetValue("Initial Catalog", out var sqlDatabase)
                        ? sqlDatabase.ToString()
                        : null;
                    Username = builder.TryGetValue("User ID", out var sqlUser) ? sqlUser.ToString() : null;
                    Password = builder.TryGetValue("Password", out var sqlPassword) ? sqlPassword.ToString() : null;
                    break;

                case ResQueueSqlEngine.Postgres:
                    builder = new NpgsqlConnectionStringBuilder(connectionString);
                    Host = builder.TryGetValue("Host", out var pgHost) ? pgHost.ToString() : null;
                    Port = builder.TryGetValue("Port", out var pgPort)
                        ? int.TryParse(pgPort.ToString(), out var port) ? port : (int?)null
                        : null;
                    Database = builder.TryGetValue("Database", out var pgDatabase) ? pgDatabase.ToString() : null;
                    Username = builder.TryGetValue("Username", out var pgUser) ? pgUser.ToString() : null;
                    Password = builder.TryGetValue("Password", out var pgPassword) ? pgPassword.ToString() : null;
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private string GenerateConnectionString()
        {
            switch (SqlEngine)
            {
                case ResQueueSqlEngine.SqlServer:
                    var sqlBuilder = new SqlConnectionStringBuilder
                    {
                        DataSource = Host,
                        InitialCatalog = Database,
                        IntegratedSecurity = false,
                        TrustServerCertificate = true
                    };

                    if (!string.IsNullOrEmpty(Username))
                    {
                        sqlBuilder.UserID = Username;
                    }

                    if (!string.IsNullOrEmpty(Password))
                    {
                        sqlBuilder.Password = Password;
                    }

                    return sqlBuilder.ConnectionString;

                case ResQueueSqlEngine.Postgres:
                    var pgBuilder = new NpgsqlConnectionStringBuilder
                    {
                        Host = Host,
                        Port = Port ?? 5432,
                        Database = Database,
                        Username = Username,
                        Password = Password
                    };

                    return pgBuilder.ConnectionString;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}