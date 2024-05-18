namespace ApiInspector.WebUI.Components;

sealed class ListView<TRecord> : Component<ListView<TRecord>.State>
{
    public required IReadOnlyList<TRecord> Records { get; init; }
    
    public IReadOnlyList<int> SelectedRecordIndexList { get; init; }
    
    protected override Element render()
    {
        return new FlexColumn(Theme.BorderRadius, Theme.Border.Component)
        {
            new FlexRowCentered(Padding(8,16),Background("#f9fafb"),BorderRadius(6))
            {
                new SearchTextBox
                {
                    Value = state.SearchText,
                    OnValueChange = x =>
                    {
                        state = state with { SearchText = x };
                        
                        return Task.CompletedTask;
                    }
                }
            },
            
            new FlexColumn(AlignItemsCenter,Gap(4), Background("white"))
            {
                Records.Select(ToElement)
            }
        };
    }
    
    Element ToElement(TRecord record)
    {
        var label = record.ToString();

        if (label is null)
        {
            return null;
        }
        
        if (state.SearchText.HasValue())
        {
            if (!label.Contains(state.SearchText))
            {
                return null;
            }
        }

        
        return new FlexRowCentered(Border(1, solid, Theme.BorderColor), BorderRadius(4), Padding(4,8), CursorDefault, Hover(Border(1,solid,"blue")))
        {
            label
        };
    }


    internal record State
    {
        public string SearchText { get; init; }
    }
}