using System.Data.SQLite;
using Dapper;
using Dapper.Contrib.Extensions;

namespace DotNetDomainBoundarySpecifier.Processors;

static class DbOperations
{
    public static Result<List<TableModel>> GetRecordsByMethod(Scope scope, string fullMethodName)
    {
        string connectionString = "Data Source=C:\\github\\DotNetDomainBoundarySpecifier\\Database.db";

        using (var connection = new SQLiteConnection(connectionString))
        {
            
            //return connection.GetAll<TableModel>().ToList();
            
            var sql = """
                      SELECT ModuleName
                             FROM ExternalDomainBoundaries
                      """;

            var r =  connection.QuerySingle<string>(sql).ToList();

            return default;
        }
    }
}