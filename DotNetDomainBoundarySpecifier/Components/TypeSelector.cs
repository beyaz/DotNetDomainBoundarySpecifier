namespace ApiInspector.WebUI.Components;

sealed class TypeSelector : Component<TypeSelector.State>
{
    public delegate Task SelectedTypeChanged(string typeFullName);

    public string AssemblyFileName { get; init; }

    public string SelectedTypeFullName { get; init; }

    [ReactCustomEvent]
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
        
        if (AssemblyFileName.HasValue())
        {
            var assemblyDefinitionResult = ReadAssemblyDefinition(Path.Combine(Config.AssemblySearchDirectory, AssemblyFileName));
            if (assemblyDefinitionResult.HasError)
            {
                return assemblyDefinitionResult.Error.ToString();
            }

            foreach (var moduleDefinition in assemblyDefinitionResult.Value.Modules)
            {
                foreach (var type in moduleDefinition.Types)
                {
                    itemsSource.Add(type.FullName);
                }
            }
        }
       

        return new ListView<string>
        {
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