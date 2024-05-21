namespace DotNetDomainBoundarySpecifier.Process;

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

sealed record GenerateDependentCodeOutput
{
    public FileModel ContractFile { get; init; }

    public FileModel ProcessFile { get; init; }
}

sealed record  TableModel
{
    public string ModuleName { get; init; }
    
    public string ExternalAssemblyFileName { get; init; }
    
    public string ExternalClassFullName { get; init; }
    
    public string ExternalMethodFullName { get; init; }
    
    public string RelatedClassFullName { get; init; }
    
    public string RelatedPropertyFullName { get; init; }
}