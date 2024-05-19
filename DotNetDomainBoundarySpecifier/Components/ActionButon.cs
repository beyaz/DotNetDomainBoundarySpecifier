namespace ApiInspector.WebUI.Components;

sealed class ActionButton : Component<ActionButton.State>
{
    public bool IsProcessing { get; init; }

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

    protected override Element render()
    {
        var isProcessing = state.IsProcessing;

        var loadingIcon = isProcessing is false ? null : new LoadingIcon { Size(20), MarginRight(10) };

        var buttonStyle = new Style
        {
            Color(BluePrimary),
            Border(1, solid, BluePrimary),
            Background(transparent),
            BorderRadius(5),
            Padding(10, 20),
            CursorPointer,
            TextAlignCenter
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
    }
}