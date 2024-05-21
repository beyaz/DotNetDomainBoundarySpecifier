namespace DotNetDomainBoundarySpecifier;

sealed record ConfigInfo
{
    // @formatting:off
    public string BrowserExeArguments { get; init; }
    public string BrowserExePath { get; init; }
    public bool HideConsoleWindow { get; init; }
    public int NextAvailablePortFrom { get; init; }
    public bool UseUrls { get; init; }

    public string AssemblySearchDirectory { get; init; }
    public IReadOnlyList<string> DomainFiles { get; init; }
    public IReadOnlyList<string> SkipTypes { get; init; }

    public FileStorageInfo FileStorage { get; init; }
    public string ModuleName { get; init; }
    public IReadOnlyList<string> ExternalDomainFileNameContains { get; init; }
    public IReadOnlyList<string> IgnoreParameterTypeNamesLike { get; init; }

    internal class FileStorageInfo
    {
        public string CacheDirectoryFormat { get; init; }
        public bool IsActive { get; init; }
    }
    
    public string ExportDirectoryForTypes { get; init; }
    public string ExportDirectoryForProcess { get; init; }
    // @formatting:on
}