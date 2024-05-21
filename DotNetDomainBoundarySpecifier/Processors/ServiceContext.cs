using ApiInspector.WebUI;

namespace DotNetDomainBoundarySpecifier.Processors;

sealed record ServiceContext
{
    public ConfigInfo Config { get; init; } = ReadConfig();
}