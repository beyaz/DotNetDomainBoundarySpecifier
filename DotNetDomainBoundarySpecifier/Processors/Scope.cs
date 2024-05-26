global using static DotNetDomainBoundarySpecifier.Processors.Scope;

namespace DotNetDomainBoundarySpecifier.Processors;

delegate IReadOnlyList<TypeDefinition> GetTypesInAssemblyFile(string assemblyFileName);

delegate MethodDefinition FindMethod(string assemblyFileName, string fullTypeName, string fullMethodName);

sealed record Scope
{
    public static readonly Scope DefaultScope = new();

    public Scope()
    {
        Config = ReadConfig();

        GetTypesInAssemblyFile = a => CecilHelper.GetTypesInAssemblyFile(this, a);

        FindMethod = (a, b, c) => CecilHelper.FindMethod(this, a, b, c);
    }

    public Config Config { get; init; }

    public GetTypesInAssemblyFile GetTypesInAssemblyFile { get; init; }

    public FindMethod FindMethod { get; init; }
}