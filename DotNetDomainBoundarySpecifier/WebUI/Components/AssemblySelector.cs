namespace DotNetDomainBoundarySpecifier.WebUI.Components;

sealed class AssemblySelector : Component<AssemblySelector.State>
{
    public delegate Task SelectedAssemblyChanged(string assemblyFileName);

    public string SelectedAssemblyFileName { get; init; }

    [CustomEvent]
    public SelectedAssemblyChanged SelectionChange { get; init; }

    protected override Task constructor()
    {
        state = new()
        {
            SelectedAssemblyFileName = SelectedAssemblyFileName
        };

        return Task.CompletedTask;
    }

    static IReadOnlyList<string> ExternalDomainAssemblyFiles => Cache.AccessValue(nameof(ExternalDomainAssemblyFiles), () =>
    {
        var config = ReadConfig();

        return Directory.GetFiles(config.AssemblySearchDirectory, "*.dll").Where(x => !IsInDomain(new(), x)).Select(Path.GetFileName).ToList();

    });
    protected override Element render()
    {
        return new ListView<string>
        {
            Title               = "Assembly",
            SelectionIsSingle   = true,
            ItemsSource         = ExternalDomainAssemblyFiles,
            SelectedItemChanged = SelectedItemChanged,
            SelectedItem        = state.SelectedAssemblyFileName
        };
    }

    static readonly CachedObjectMap Cache = new ();

    Task SelectedItemChanged(string selectedItem)
    {
        state = state with
        {
            SelectedAssemblyFileName = selectedItem
        };

        DispatchEvent(SelectionChange, [state.SelectedAssemblyFileName]);

        return Task.CompletedTask;
    }

    internal record State
    {
        public string SelectedAssemblyFileName { get; init; }
    }
}