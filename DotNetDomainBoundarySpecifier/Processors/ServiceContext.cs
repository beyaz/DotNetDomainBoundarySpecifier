namespace DotNetDomainBoundarySpecifier.Processors;

sealed record ServiceContext
{
    public ConfigInfo Config { get; init; } = ReadConfig();

    public Func<string, IReadOnlyList<TypeDefinition>> GetTypesInAssemblyFile { get; init; }

    public ServiceContext()
    {
        GetTypesInAssemblyFile = assemblyFileName => CecilHelper.GetTypesInAssemblyFile(this, assemblyFileName);
    }
}