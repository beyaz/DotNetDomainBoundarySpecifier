namespace ApiInspector.WebUI.Components;

sealed class AssemblySelector : Component<AssemblySelector.State>
{
    public delegate Task SelectedAssemblyChanged(string assemblyFileName);

    protected override Element render()
    {
        var itemsSource = Directory.GetFiles(Config.AssemblySearchDirectory, "*.dll").Where(x => !isInDomain(x)).Select(Path.GetFileName).ToList();

        var selectedIndexList = new List<int>();
        
        if (state.SelectedAssemblyFileName.HasValue())
        {
            var index = itemsSource.IndexOf(state.SelectedAssemblyFileName);
            if (index >= 0)
            {
                selectedIndexList = [index];
            }
        }
        
        return new ListView<string>
        {
            ItemsSource          = itemsSource,
            SelectedItemsChanged = SelectedItemsChanged,
            IndexListOfSelectedItems = selectedIndexList
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