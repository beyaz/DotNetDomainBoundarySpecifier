namespace DotNetDomainBoundarySpecifier.Processors;

sealed record Scope
{
    public Scope()
    {
        Config = ReadConfig();

        GetTypesInAssemblyFile = assemblyFileName => CecilHelper.GetTypesInAssemblyFile(this, assemblyFileName);
    }

    public Config Config { get; init; }

    public Func<string, IReadOnlyList<TypeDefinition>> GetTypesInAssemblyFile { get; init; }
}