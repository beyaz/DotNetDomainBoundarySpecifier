namespace DotNetDomainBoundarySpecifier.WebUI.Components;

sealed class MethodSelector : Component<MethodSelector.State>
{
    public delegate Task SelectedMethodChanged(string typeFullName);

    public string SelectedAssemblyFileName { get; init; }

    public string SelectedMethodFullName { get; init; }

    public string SelectedTypeFullName { get; init; }

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
        var markedItems = new List<string>();

        if (SelectedAssemblyFileName.HasValue())
        {
            var config = ReadConfig();

            var assemblyDefinitionResult = ReadAssemblyDefinition(Path.Combine(config.AssemblySearchDirectory, SelectedAssemblyFileName));
            if (assemblyDefinitionResult.HasError)
            {
                return assemblyDefinitionResult.Error.ToString();
            }

            itemsSource = GetMethods(assemblyDefinitionResult.Value, SelectedTypeFullName).Select(m => m.FullName).ToList();

            markedItems = GetCalledMethodsFromExternalDomain(new(), SelectedAssemblyFileName)
                         .Where(m => m.DeclaringType.FullName == SelectedTypeFullName)
                         .Select(m => m.FullName).ToList();
        }

        return new ListView<string>
        {
            Name = "Method",
            Title               = "Method",
            SelectionIsSingle   = true,
            ItemsSource         = itemsSource,
            MarkedItems         = markedItems,
            SelectedItemChanged = SelectedItemChanged,
            SelectedItem        = state.SelectedMethodFullName
        };
    }

    static IEnumerable<MethodDefinition> GetMethods(AssemblyDefinition assemblyDefinition, string filterTypeFullName)
    {
        foreach (var moduleDefinition in assemblyDefinition.Modules)
        {
            foreach (var type in moduleDefinition.Types)
            {
                if (type.FullName == filterTypeFullName)
                {
                    foreach (var methodDefinition in type.Methods)
                    {
                        if (methodDefinition.IsConstructor)
                        {
                            continue;
                        }

                        yield return methodDefinition;
                    }
                }
            }
        }
    }

    Task SelectedItemChanged(string selectedItem)
    {
        state = new()
        {
            SelectedMethodFullName = selectedItem
        };

        DispatchEvent(SelectionChange, [state.SelectedMethodFullName]);

        return Task.CompletedTask;
    }

    internal record State
    {
        public string SelectedMethodFullName { get; init; }
    }
}