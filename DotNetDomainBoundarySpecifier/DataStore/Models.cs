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



