global using static DotNetDomainBoundarySpecifier.Processors.Scope;

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