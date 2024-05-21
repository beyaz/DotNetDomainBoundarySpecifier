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

    protected override Element render()
    {
        var config = ReadConfig();

        var itemsSource = Directory.GetFiles(config.AssemblySearchDirectory, "*.dll").Where(x => !IsInDomain(new(), x)).Select(Path.GetFileName).ToList();

        return new ListView<string>
        {
            Title               = "Assembly",
            SelectionIsSingle   = true,
            ItemsSource         = itemsSource,
            SelectedItemChanged = SelectedItemChanged,
            SelectedItem        = state.SelectedAssemblyFileName
        };
    }

    Task SelectedItemChanged(string selecteditem)
    {
        state = state with
        {
            SelectedAssemblyFileName = selecteditem
        };

        DispatchEvent(SelectionChange, [state.SelectedAssemblyFileName]);

        return Task.CompletedTask;
    }

    internal record State
    {
        public string SelectedAssemblyFileName { get; init; }
    }
}