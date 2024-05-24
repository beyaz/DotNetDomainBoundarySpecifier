using System.Data.Common;
using System.Data.SQLite;
using Dapper;
using Dapper.Contrib.Extensions;

namespace DotNetDomainBoundarySpecifier.Processors;

static class Db
{
    public static Result<Option<ExternalDomainBoundary>> TryGet(Scope scope, ExternalDomainBoundaryMethod method)
    {
        return Operation(scope, db =>
        {
            const string sql =
                $"""
                 SELECT *
                   FROM {nameof(ExternalDomainBoundaryMethod)}
                  WHERE {nameof(ExternalDomainBoundaryMethod.ExternalAssemblyFileName)} = @{nameof(method.ExternalAssemblyFileName)}
                    AND {nameof(ExternalDomainBoundaryMethod.ExternalClassFullName)} = @{nameof(method.ExternalClassFullName)}
                    AND {nameof(ExternalDomainBoundaryMethod.ExternalMethodFullName)} = @{nameof(method.ExternalMethodFullName)}
                 """;

            var externalMethod = db.QueryFirstOrDefault<ExternalDomainBoundaryMethod>(sql, method);

            if (externalMethod is null)
            {
                return None;
            }
            
            const string sql2 =
                $"""
                 SELECT *
                   FROM {nameof(ExternalDomainBoundaryProperty)}
                  WHERE {nameof(ExternalDomainBoundaryProperty.MethodId)} = @{nameof(externalMethod.RecordId)}
                 """;

            var externalProperties = db.Query<ExternalDomainBoundaryProperty>(sql2, externalMethod.RecordId);

            return Some(new ExternalDomainBoundary
            {
                Method     = externalMethod,
                Properties = externalProperties.ToImmutableList()
            });
        });
    }
    
    public static Result<long> Save(Scope scope, ExternalDomainBoundary records)
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