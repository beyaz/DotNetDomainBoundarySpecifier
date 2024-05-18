namespace ApiInspector.WebUI.Components;

sealed class ListView<TRecord> : Component<ListView<TRecord>.State>
{
    public required IReadOnlyList<TRecord> Records { get; init; }
    
    public IReadOnlyList<int> SelectedRecordIndexList { get; init; }
    
    protected override Element render()
    {
        return new FlexColumn
        {
            new FlexRowCentered(Padding(8,16),Background("#f9fafb"))
            {
                new FlexRow
                {
                    new input
                    {
                        type                     = "text",
                        placeholder              = "Search...",
                        valueBind                = () => state.SearchText,
                        valueBindDebounceTimeout = 700,
                        valueBindDebounceHandler = OnSearchKeyPressFinished,
                        style =
                        {
                            BorderNone
                        }
                    },
                    Theme.SearchIcon,
                    
                    new Style
                    {
                        WidthFull,
                        Border(1,solid,"rgb(209, 213, 219)"),
                        BorderRadius(6),
                        Padding(10)
                    }
                }
            },
            
            new FlexColumn(AlignItemsCenter,Gap(4))
            {
                Records.Select(ToElement)
            }
        };
    }
    
    Element ToElement(TRecord record)
    {
        return new FlexRowCentered(Border(1, solid, Theme.BorderColor), BorderRadius(4), Padding(4,8), CursorDefault, Hover(Border(1,solid,"blue")))
        {
            record.ToString()
        };
    }

    Task OnSearchKeyPressFinished()
    {
        return Task.CompletedTask;
    }

    internal class State
    {
        public string SearchText { get; init; }
    }
}