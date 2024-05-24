using System.Data.Common;
using System.Data.SQLite;
using Dapper;
using Dapper.Contrib.Extensions;

namespace DotNetDomainBoundarySpecifier.Processors;

static class Db
{
    public static Result<List<ExternalDomainBoundary>> GetRecordsByMethod(Scope scope, string externalMethodFullName)
    {
        return Operation(scope, db =>
        {
            const string sql =
                $"""
                 SELECT *
                   FROM {nameof(ExternalDomainBoundary)}
                  WHERE {nameof(ExternalDomainBoundary.ExternalMethodFullName)} = @{nameof(externalMethodFullName)}
                 """;

            return db.Query<ExternalDomainBoundary>(sql, new { externalMethodFullName }).ToList();
        });
    }
    
    public static Result<long> Save(Scope scope, List<ExternalDomainBoundary> records)
    {
        return Operation(scope, db => db.Insert(records));
    }

    static string ConnectionString(Scope scope)
    {
        return $"Data Source={scope.Config.DatabaseFilePath}";
    }

    static Result<T> Operation<T>(Scope scope, Func<DbConnection, T> operation)
    {
        using (var connection = new SQLiteConnection(ConnectionString(scope)))
        {
            return operation(connection);
        }
    }
}