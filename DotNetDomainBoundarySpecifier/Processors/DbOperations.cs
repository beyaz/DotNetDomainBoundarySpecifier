using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;

namespace DotNetDomainBoundarySpecifier.Processors;

static class DbOperations
{
    public static Result<List<TableModel>> GetRecordsByMethod(Scope scope, string fullMethodName)
    {
        string connectionString = "Data Source=Database.db";

        using (var connection = new SqliteConnection(connectionString))
        {
            
            return connection.GetAll<TableModel>().ToList();
            
            var sql = """
                      SELECT *
                             FROM ExternalDomainBoundaries
                      """;

            return connection.Query<TableModel>(sql).ToList();
        }
    }
}