namespace DotNetDomainBoundarySpecifier.DataStore;

sealed record  TableModel
{
    public string ModuleName { get; init; }
    
    public string ExternalAssemblyFileName { get; init; }
    
    public string ExternalClassFullName { get; init; }
    
    public string ExternalMethodFullName { get; init; }
    
    public string RelatedClassFullName { get; init; }
    
    public string RelatedPropertyFullName { get; init; }
}

sealed record  MethodModel
{
    public string ModuleName { get; init; }
    
    public string FullName { get; init; }
    
    public string MethodName { get; init; }
}

sealed record  PropertyModel
{
    public string TypeName { get; init; }
    
    public string Name { get; init; }
}

sealed record  ClassModel
{
    public string AssemblyFile { get; init; }
    
    public string FullName { get; init; }
    
    public string ExportName { get; init; }
}

sealed record  ApiAggregate
{
    public MethodModel MethodModel { get; init; }
    
    public IReadOnlyList<ClassModel> RelatedClassList { get; init; }
    
    public IReadOnlyList<PropertyModel> RelatedPropertyList { get; init; }
}