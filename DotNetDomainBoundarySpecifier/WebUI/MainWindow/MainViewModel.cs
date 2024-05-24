namespace DotNetDomainBoundarySpecifier.WebUI.MainWindow;

sealed record MainViewModel
{
    public string SelectedAssemblyFileName { get; init; }

    public string SelectedTypeFullName { get; init; }

    public string SelectedMethodFullName { get; init; }
    
    public ExternalDomainBoundary Records { get; init; }

    public string GeneratedCode { get; init; }

    public int HasTransaction { get; init; }

    public bool IsAnalyzing { get; init; }
    
    public bool IsSaving { get; init; }
    
    public bool IsExporting { get; init; }
}