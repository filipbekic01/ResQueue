using System.Data.Common;

namespace ResQueue.Factories;

public interface IDatabaseConnectionFactory
{
    DbConnection CreateConnection();
    DbCommand CreateCommand(string commandText, DbConnection connection);
}