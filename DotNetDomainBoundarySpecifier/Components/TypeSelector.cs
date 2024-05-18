namespace ApiInspector.WebUI.Components;

sealed class TypeSelector : Component<TypeSelector.State>
{
    public delegate Task SelectedAssemblyChanged(string assemblyFileName);

    public string AssemblyFileName { get; init; } = "Test.DomainB.dll";
    
    protected override Element render()
    {
        var sourceAssemblyDefinitionResult = ReadAssemblyDefinition(Path.Combine(Config.AssemblySearchDirectory, AssemblyFileName));
        if (sourceAssemblyDefinitionResult.HasError)
        {
            
        }
        
        var itemsSource = new List<string>();
        
        foreach (var moduleDefinition in sourceAssemblyDefinitionResult.Value.Modules)
        {
            foreach (var type in moduleDefinition.Types)
            {
                itemsSource.Add(type.FullName);
            }
        }
        
        return new ListView<string>
        {
            SelectionIsSingle    = true,
            ItemsSource          = itemsSource,
            SelectedItemsChanged = SelectedItemsChanged,
            SelectedItems        = state.SelectedAssemblyFileName.HasNoValue() ? [] : [state.SelectedAssemblyFileName]
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