global using static DotNetDomainBoundarySpecifier.Processors.Scope;
using Microsoft.Data.Sqlite;

namespace DotNetDomainBoundarySpecifier.Processors;

sealed record Scope
{
    public static readonly Scope DefaultScope = new();

    public Scope()
    {
        Config = ReadConfig();

        GetTypesInAssemblyFile = assemblyFileName => CecilHelper.GetTypesInAssemblyFile(this, assemblyFileName);
    }

    public Config Config { get; init; }

    public Func<string, IReadOnlyList<TypeDefinition>> GetTypesInAssemblyFile { get; init; }
}


static class DbOperations
{
    //public static Result<List<TableModel>> GetRecordsByMethod(Scope scope, string fullMethodName)
    //{
    //    string connectionString = "Data Source=Database.db";

    //    var sqliteConnection = new SqliteConnection(connectionString);
        
    //    DbConnetionTypedatabaseType = DbConnetionType.Sqlite; 
    //}
}