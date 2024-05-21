namespace ApiInspector.WebUI.Components;

sealed class TypeSelector : Component<TypeSelector.State>
{
    public delegate Task SelectedTypeChanged(string typeFullName);

    public string SelectedAssemblyFileName { get; init; }

    public string SelectedTypeFullName { get; init; }

    [CustomEvent]
    public SelectedTypeChanged SelectionChange { get; init; }

    protected override Task constructor()
    {
        state = new()
        {
            SelectedTypeFullName = SelectedTypeFullName
        };

        return Task.CompletedTask;
    }

    protected override Element render()
    {
        var itemsSource = new List<string>();

        if (SelectedAssemblyFileName.HasValue())
        {
            var config = ReadConfig();
            
            var filePath = Path.Combine(config.AssemblySearchDirectory, SelectedAssemblyFileName);

            itemsSource = GetTypesInAssemblyFile(new(),filePath).Select(x => x.FullName).ToList();
        }

        return new ListView<string>
        {
            Title = "Class",
            SelectionIsSingle   = true,
            ItemsSource         = itemsSource,
            SelectedItemChanged = SelectedItemChanged,
            SelectedItem        = state.SelectedTypeFullName
        };
    }

    Task SelectedItemChanged(string selecteditem)
    {
        state = state with
        {
            SelectedTypeFullName = selecteditem
        };

        DispatchEvent(SelectionChange, [state.SelectedTypeFullName]);

        return Task.CompletedTask;
    }

    internal record State
    {
        public string SelectedTypeFullName { get; init; }
    }
}