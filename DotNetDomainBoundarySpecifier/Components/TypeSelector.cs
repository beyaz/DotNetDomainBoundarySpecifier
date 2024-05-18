namespace ApiInspector.WebUI.Components;

sealed class TypeSelector : Component<TypeSelector.State>
{
    public delegate Task SelectedTypeChanged(string typeFullName);

    public string AssemblyFileName { get; init; } = "Test.DomainB.dll";
    
    protected override Element render()
    {
        var sourceAssemblyDefinitionResult = ReadAssemblyDefinition(Path.Combine(Config.AssemblySearchDirectory, AssemblyFileName));
        if (sourceAssemblyDefinitionResult.HasError)
        {
            return sourceAssemblyDefinitionResult.Error.ToString();
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
            SelectedItemChanged = SelectedItemChanged,
            SelectedItem         = state.SelectedTypeFullName
        };
    }

    Task SelectedItemChanged(string selecteditem)
    {
        state = state with
        {
            SelectedTypeFullName = selecteditem
        };

        Client.DispatchEvent<SelectedTypeChanged>([state.SelectedTypeFullName]);

        return Task.CompletedTask;
    }

    internal record State
    {
        public string SelectedTypeFullName { get; init; }
    }
}