namespace ApiInspector.WebUI;

public sealed record MainWindowModel
{
    public string SelectedAssemblyFileName { get; init; }
    
    public string SelectedTypeFullName { get; init; }
    
    public string SelectedMethodFullName { get; init; }
    
}