﻿namespace DotNetDomainBoundarySpecifier.WebUI.Components;

sealed class AssemblySelector : Component<AssemblySelector.State>
{
    internal static readonly IReadOnlyList<string> ExternalDomainAssemblyFiles
        = Directory.GetFiles(ReadConfig().AssemblySearchDirectory, "*.dll")
                   .Where(x => !IsInDomain(DefaultScope, x))
                   .Select(Path.GetFileName)
                   .ToList();

    public delegate Task SelectedAssemblyChanged(string assemblyFileName);

    public string SelectedAssemblyFileName { get; init; }

    [CustomEvent]
    public SelectedAssemblyChanged SelectionChange { get; init; }

    protected override Task constructor()
    {
        state = new()
        {
            SelectedAssemblyFileName = SelectedAssemblyFileName
        };

        return Task.CompletedTask;
    }

    protected override Element render()
    {
        return new ListView<string>
        {
            Name = "Assembly",
            Title               = "Assembly",
            SelectionIsSingle   = true,
            ItemsSource         = ExternalDomainAssemblyFiles,
            SelectedItemChanged = SelectedItemChanged,
            SelectedItem        = state.SelectedAssemblyFileName
        };
    }

    Task SelectedItemChanged(string senderName, string selectedItem)
    {
        state = new()
        {
            SelectedAssemblyFileName = selectedItem
        };

        DispatchEvent(SelectionChange, [state.SelectedAssemblyFileName]);

        return Task.CompletedTask;
    }

    internal record State
    {
        public string SelectedAssemblyFileName { get; init; }
    }
}