using System.Data.Common;
using System.Data.SQLite;
using Dapper;
using Dapper.Contrib.Extensions;

namespace DotNetDomainBoundarySpecifier.Processors;

static class Db
{
    public static Result<ExternalDomainBoundary> Save(Scope scope, ExternalDomainBoundary boundary)
    {
        return Operation(scope, db =>
        {
            TryGet(scope, boundary.Method).Then(x =>
            {
                db.Delete(x.Properties);
                db.Delete(x.Method);
            });

            var method = boundary.Method with
            {
                RecordId = db.Insert(boundary.Method)
            };

            var properties = boundary.Properties.Select(p => p with { MethodId = boundary.Method.RecordId }).Select(p => p with
            {
                RecordId = db.Insert(p)
            });

            return new ExternalDomainBoundary
            {
                Method     = method,
                Properties = properties.ToImmutableList()
            };
        });
    }

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