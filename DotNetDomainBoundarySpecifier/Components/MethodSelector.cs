namespace ApiInspector.WebUI.Components;

sealed class MethodSelector : Component<MethodSelector.State>
{
    public delegate Task SelectedMethodChanged(string typeFullName);

    public string SelectedAssemblyFileName { get; init; }

    public string SelectedTypeFullName { get; init; }
    
    public string SelectedMethodFullName { get; init; }

    [CustomEvent]
    public SelectedMethodChanged SelectionChange { get; init; }

    protected override Task constructor()
    {
        state = new()
        {
            SelectedMethodFullName = SelectedMethodFullName
        };

        return Task.CompletedTask;
    }

    protected override Element render()
    {
        var itemsSource = new List<string>();

        if (SelectedAssemblyFileName.HasValue())
        {
            var assemblyDefinitionResult = ReadAssemblyDefinition(Path.Combine(Config.AssemblySearchDirectory, SelectedAssemblyFileName));
            if (assemblyDefinitionResult.HasError)
            {
                return assemblyDefinitionResult.Error.ToString();
            }

            foreach (var moduleDefinition in assemblyDefinitionResult.Value.Modules)
            {
                foreach (var type in moduleDefinition.Types)
                {
                    if (type.FullName == SelectedTypeFullName)
                    {
                        foreach (var methodDefinition in type.Methods)
                        {
                            if (methodDefinition.IsConstructor)
                            {
                                continue;
                            }
                            itemsSource.Add(methodDefinition.FullName);     
                        }
                    }
                }
            }
        }

        return new ListView<string>
        {
            SelectionIsSingle   = true,
            ItemsSource         = itemsSource,
            SelectedItemChanged = SelectedItemChanged,
            SelectedItem        = state.SelectedMethodFullName
        };
    }

    Task SelectedItemChanged(string selecteditem)
    {
        state = state with
        {
            SelectedMethodFullName = selecteditem
        };

        DispatchEvent(SelectionChange, [state.SelectedMethodFullName]);

        return Task.CompletedTask;
    }

    internal record State
    {
        public string SelectedMethodFullName { get; init; }
    }
}