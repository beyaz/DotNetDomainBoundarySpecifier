namespace DotNetDomainBoundarySpecifier.WebUI.MainWindow;

sealed record MainViewModel
{
    public string SelectedAssemblyFileName { get; init; }

    public string SelectedTypeFullName { get; init; }

    public string SelectedMethodFullName { get; init; }

    public bool IsAnalyzing { get; init; }

    public ImmutableList<ExternalDomainBoundary> Records { get; init; } = ImmutableList<ExternalDomainBoundary>.Empty;

    public string GeneratedCode { get; init; }

    public int HasTransaction { get; init; }
}