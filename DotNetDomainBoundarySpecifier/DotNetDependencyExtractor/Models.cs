using System.Collections.Immutable;

namespace DotNetDependencyExtractor;

sealed record AssemblyAnalyse
{
    public AssemblyDefinition AssemblyDefinition { get; init; }

    public IReadOnlyList<TypeDefinition> Types { get; init; }

    public IReadOnlyList<MethodReference> CalledMethods { get; init; }
}

sealed record TypeExportInfo
{
    public string ClassName { get; init; }

    public bool ExportAsEnum { get; init; }

    public TypeDefinition TypeDefinition { get; init; }

    public IReadOnlyList<PropertyDefinition> UsedProperties { get; init; }
}

sealed record TypeExportContext
{
    public ImmutableList<TypeExportInfo> ExportList { get; init; }

    public IReadOnlyList<AssemblyAnalyse> CardOrchestrations { get; init; }
    
    public IReadOnlyList<AssemblyAnalyse> CardSystemSearchFiles { get; init; }
}

sealed record FileModel
{
    public string Name { get; init; }

    public string Content { get; init; }

    public string DirectoryPath { get; init; }
} 

sealed record GenerateDependentCodeInput
{
    public string TargetAssembly { get; init; }

    public string TargetMethodName { get; init; }

    public string TargetTypeFullName { get; init; }

    internal AssemblyDefinition TargetAssemblyDefinition { get; init; }

    internal MethodDefinition TargetMethod { get; init; }

    internal TypeDefinition TargetType { get; init; }
    
    internal IReadOnlyList<AssemblyAnalyse> CardSystemSearchFiles { get; init; }
}

sealed record GenerateDependentCodeOutput
{
    public FileModel ContractFile { get; init; }

    public FileModel ProcessFile { get; init; }
}