namespace DotNetDomainBoundarySpecifier.WebUI.MainWindow;

sealed record MainViewModel
{
    public string SelectedAssemblyFileName { get; init; }

    public string SelectedClassFullName { get; init; }

    public string SelectedMethodFullName { get; init; }
    
    public ExternalDomainBoundary Boundary { get; init; }

    public string GeneratedCode { get; init; }

    public int HasTransaction { get; init; }

    public bool IsAnalyzing { get; init; }
    
    public bool IsSaving { get; init; }
    
    public bool IsExporting { get; init; }
}