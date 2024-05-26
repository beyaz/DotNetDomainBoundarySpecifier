global using static DotNetDomainBoundarySpecifier.Processors.Scope;

namespace DotNetDomainBoundarySpecifier.Processors;

delegate IReadOnlyList<TypeDefinition> GetTypesInAssemblyFile(string assemblyFileName);

sealed record Scope
{
    public static readonly Scope DefaultScope = new();

    public Scope()
    {
        Config = ReadConfig();

        GetTypesInAssemblyFile = assemblyFileName => CecilHelper.GetTypesInAssemblyFile(this, assemblyFileName);
    }

    public Config Config { get; init; }

    public GetTypesInAssemblyFile GetTypesInAssemblyFile { get; init; }


    public MethodDefinition FindMethod(string assemblyFileName, string fullTypeName, string fullMethodName)
    {
        return GetTypesInAssemblyFile(assemblyFileName)
            .FirstOrDefault(t => t.FullName == fullTypeName)?
            .Methods.FirstOrDefault(m => m.FullName == fullMethodName);
    }
}


