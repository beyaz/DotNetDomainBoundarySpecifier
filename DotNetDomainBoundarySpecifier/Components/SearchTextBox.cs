namespace ApiInspector.WebUI.Components;

sealed class SearchTextBox : Component<SearchTextBox.State>
{
    [ReactCustomEvent]
    public Func<string, Task> OnValueChange { get; init; }

    public string Value { get; init; }

    protected override Task constructor()
    {
        state = new()
        {
            InitialValue = Value,
            Value        = Value
        };

        return Task.CompletedTask;
    }

    protected override Element render()
    {
        return new FlexRow(WidthFull, Background("white"), Gap(6))
        {
            Theme.SearchIcon,
            new input
            {
                type                     = "text",
                placeholder              = "Search...",
                valueBind                = () => state.Value,
                valueBindDebounceTimeout = 700,
                valueBindDebounceHandler = OnSearchKeyPressFinished,
                style =
                {
                    BorderNone,
                    Color(rgb(75, 85, 99))
                }
            },

            new Style
            {
                WidthFull,
                Border(1, solid, "rgb(209, 213, 219)"),
                BorderRadius(6),
                Padding(10)
            }
        };
    }

    Task OnSearchKeyPressFinished()
    {
        DispatchEvent(OnValueChange, [state.Value]);

        return Task.CompletedTask;
    }

    internal sealed record State
    {
        public string InitialValue { get; init; }

        public string Value { get; init; }
    }
}