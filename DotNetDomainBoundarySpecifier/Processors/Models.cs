﻿using System.Diagnostics;
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

[DebuggerDisplay("{Method} -> {Properties}")]
sealed record ExternalDomainBoundary
{
    public ExternalDomainBoundaryMethod Method { get; init; }
    
    public ImmutableList<ExternalDomainBoundaryProperty> Properties { get; init; } = ImmutableList<ExternalDomainBoundaryProperty>.Empty;
}

[Table(nameof(ExternalDomainBoundaryMethod))]
sealed record ExternalDomainBoundaryMethod
{
    
    [Key]
    public long RecordId { get; init; }
    
    public string ModuleName { get; init; }

    public string ExternalAssemblyFileName { get; init; }

    public string ExternalClassFullName { get; init; }

    public string ExternalMethodFullName { get; init; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        
        if (ExternalClassFullName.HasValue())
        {
            sb.Append(ExternalClassFullName.Split('.', StringSplitOptions.RemoveEmptyEntries).Last());
            sb.Append(" -> ");
        }
        
        if (ExternalMethodFullName.HasValue())
        {
            sb.Append(ExternalMethodFullName.Split(":()".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last());
        }

        return sb.ToString();
    }
}

[Table(nameof(ExternalDomainBoundaryProperty))]
sealed record ExternalDomainBoundaryProperty
{
    [Key]
    public long RecordId { get; init; }
    
    public long MethodId { get; init; }

    public string AssemblyFileName { get; init; }
    
    public string RelatedClassFullName { get; init; }

    public string RelatedPropertyName { get; init; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        
        if (RelatedClassFullName.HasValue())
        {
            sb.Append(RelatedClassFullName.Split('.', StringSplitOptions.RemoveEmptyEntries).Last());
            sb.Append(" -> ");
        }
        
        if (RelatedPropertyName.HasValue())
        {
            sb.Append(RelatedPropertyName.Split(":()".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last());
        }

        return sb.ToString();
    }
}