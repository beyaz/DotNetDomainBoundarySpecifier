namespace ApiInspector.WebUI;

sealed record MainWindowModel
{
    public string SelectedAssemblyFileName { get; init; }
    
    public string SelectedTypeFullName { get; init; }
    
    public string SelectedMethodFullName { get; init; }
    public bool IsAnalyzing { get; init; }
    
    public ImmutableList<TableModel> Records { get; init; }
    public string GeneratedCode { get; init; }
}