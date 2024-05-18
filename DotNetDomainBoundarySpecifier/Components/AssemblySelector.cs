namespace ApiInspector.WebUI.Components;

sealed class AssemblySelector : Component<AssemblySelector.State>
{
    public delegate Task SelectedAssemblyChanged(string assemblyFileName);

    protected override Element render()
    {
        var itemsSource = Directory.GetFiles(Config.AssemblySearchDirectory, "*.dll").Where(x => !isInDomain(x)).Select(Path.GetFileName).ToList();
        
        return new ListView<string>
        {
            SelectionIsSingle = true,
            ItemsSource          = itemsSource,
            SelectedItemsChanged = SelectedItemsChanged,
            SelectedItems = state.SelectedAssemblyFileName.HasNoValue() ? [] : [state.SelectedAssemblyFileName]
        };
    }

    Task SelectedItemsChanged(IReadOnlyList<string> selecteditems)
    {
        state = state with
        {
            SelectedAssemblyFileName = selecteditems.FirstOrDefault()
        };

        Client.DispatchEvent<SelectedAssemblyChanged>([state.SelectedAssemblyFileName]);

        return Task.CompletedTask;
    }

    internal record State
    {
        public string SelectedAssemblyFileName { get; init; }
    }
}