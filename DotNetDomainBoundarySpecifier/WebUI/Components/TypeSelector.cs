namespace DotNetDomainBoundarySpecifier.WebUI.Components;

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
        var markedItems = new List<string>();

        if (SelectedAssemblyFileName.HasValue())
        {
            itemsSource = DefaultScope.GetTypesInAssemblyFile(SelectedAssemblyFileName).Select(x => x.FullName).ToList();

            markedItems = GetCalledMethodsFromExternalDomain(DefaultScope, SelectedAssemblyFileName)
                         .Select(m => m.DeclaringType.FullName).ToList();
        }

        return new ListView<string>
        {
            Name = "Class",
            Title               = "Class",
            SelectionIsSingle   = true,
            ItemsSource         = itemsSource,
            MarkedItems         = markedItems,
            SelectedItemChanged = SelectedItemChanged,
            SelectedItem        = state.SelectedTypeFullName
        };
    }

    Task SelectedItemChanged(string senderName, string selectedItem)
    {
        state = new()
        {
            SelectedTypeFullName = selectedItem
        };

        DispatchEvent(SelectionChange, [state.SelectedTypeFullName]);

        return Task.CompletedTask;
    }

    internal record State
    {
        public string SelectedTypeFullName { get; init; }
    }
}