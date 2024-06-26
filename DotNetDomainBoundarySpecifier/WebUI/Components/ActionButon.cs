﻿namespace DotNetDomainBoundarySpecifier.WebUI.Components;

sealed class ActionButton : Component<ActionButton.State>
{
    public bool IsProcessing { get; init; }
    
    public bool IsDisabled { get; init; }

    public string Label { get; init; }

    [CustomEvent]
    public Func<Task> OnClicked { get; init; }

    protected override Task constructor()
    {
        state = new()
        {
            IsProcessingInitialValue = IsProcessing,
            IsProcessing             = IsProcessing
        };

        return Task.CompletedTask;
    }

    protected override Task OverrideStateFromPropsBeforeRender()
    {
        if (IsProcessing && state.IsProcessing)
        {
            state = state with { ShouldResetStateNextRender = true };

            return Task.CompletedTask;
        }

        if (state.ShouldResetStateNextRender && !IsProcessing)
        {
            state = new();
        }

        return Task.CompletedTask;
    }

    protected override Element render()
    {
        var isProcessing = state.IsProcessing;

        var loadingIcon = isProcessing is false ? null : new LoadingIcon { Size(20), MarginRight(10) };

        var buttonStyle = new Style
        {
            Color(Theme.BluePrimary),
            Border(1, solid, Theme.BluePrimary),
            Background(transparent),
            BorderRadius(5),
            Padding(10, 20),
            CursorPointer,
            TextAlignCenter,
            IsDisabled ? Cursor("not-allowed") : null,
            IsDisabled ? Opacity(0.3) : null
        };

        var onClick = isProcessing ? null : OnClick(ActionButtonOnClick);

        return new FlexRowCentered(buttonStyle, onClick)
        {
            loadingIcon,
            Label
        };
    }

    Task ActionButtonOnClick(MouseEvent _)
    {
        state = state with { IsProcessing = true };

        DispatchEvent(OnClicked);

        return Task.CompletedTask;
    }

    internal record State
    {
        public bool IsProcessing { get; init; }

        public bool IsProcessingInitialValue { get; init; }

        public bool ShouldResetStateNextRender { get; init; }
    }
}