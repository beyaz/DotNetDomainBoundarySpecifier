namespace ApiInspector.WebUI.Components;

sealed class AssemblySelector : Component<AssemblySelector.State>
{
    public delegate Task SelectedAssemblyChanged(string assemblyFileName);

    protected override Element render()
    {
        var itemsSource = Directory.GetFiles(Config.AssemblySearchDirectory, "*.dll").Where(x => !isInDomain(x)).Select(Path.GetFileName).ToList();

        return new ListView<string>
        {
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

        Client.DispatchEvent<SelectedAssemblyChanged>([state.SelectedAssemblyFileName]);

        return Task.CompletedTask;
    }

    internal record State
    {
        public string SelectedAssemblyFileName { get; init; }
    }
}