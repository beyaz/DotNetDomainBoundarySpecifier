using System.Diagnostics;
using Dapper.Contrib.Extensions;

namespace DotNetDomainBoundarySpecifier.Processors;

sealed record AssemblyAnalyse
{
    public AssemblyDefinition AssemblyDefinition { get; init; }

    public IReadOnlyList<TypeDefinition> Types { get; init; }

    public IReadOnlyList<MethodReference> CalledMethods { get; init; }
}

sealed record FileModel
{
    public string Name { get; init; }

    public string Content { get; init; }

    public string DirectoryPath { get; init; }
}

sealed record CodeGenerationOutput
{
    public FileModel ContractFile { get; init; }

    public FileModel ProcessFile { get; init; }
}


[Table("ExternalDomainBoundaries")]
sealed record TableModel
{
    /*
     CREATE TABLE ExternalDomainBoundaries (
           RecordId INTEGER PRIMARY KEY AUTOINCREMENT,
           ModuleName               TEXT (250),
           ExternalAssemblyFileName TEXT (500),
           ExternalClassFullName    TEXT (1000),
           ExternalMethodFullName   TEXT (1000),
           RelatedClassFullName     TEXT (1000),
           RelatedPropertyFullName  TEXT (1000) 
       );
     */

    [Key]
    public int RecordId { get; init; }
    
    public string ModuleName { get; init; }

    public string ExternalAssemblyFileName { get; init; }

    public string ExternalClassFullName { get; init; }

    public string ExternalMethodFullName { get; init; }

    public string RelatedClassFullName { get; init; }

    public string RelatedPropertyFullName { get; init; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        
        if (RelatedClassFullName.HasValue())
        {
            sb.Append(RelatedClassFullName.Split('.', StringSplitOptions.RemoveEmptyEntries).Last());
            sb.Append(" -> ");
        }
        
        if (RelatedPropertyFullName.HasValue())
        {
            sb.Append(RelatedPropertyFullName.Split(":()".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last());
        }

        return sb.ToString();
    }
}