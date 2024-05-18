namespace ApiInspector.WebUI.Components;

sealed class ListView<TRecord> : Component<ListView<TRecord>.State>
{
    public required IReadOnlyList<TRecord> Records { get; init; }

    public IReadOnlyList<int> SelectedRecordIndexList { get; init; } = [];
    
    protected override Element render()
    {
        return new FlexColumn(BorderRadius(Theme.BorderRadius), Theme.Border)
        {
            new FlexRowCentered(Padding(8,16),Background("#f9fafb"),BorderRadius(Theme.BorderRadius,Theme.BorderRadius,0,0))
            {
                new SearchTextBox
                {
                    Value = state.SearchText,
                    OnValueChange = x =>
                    {
                        state = state with { SearchText = x };
                        
                        return Task.CompletedTask;
                    }
                },
                new Style
                {
                    BorderBottom(1,solid,Theme.BorderColor)
                }
            },
            
            new FlexColumn(AlignItemsCenter, PaddingTopBottom(4), Gap(8), Background("white"), BorderBottomLeftRadius(Theme.BorderRadius), BorderBottomRightRadius(Theme.BorderRadius))
            {
                Records.Select(ToElement)
            }
        };
    }
    
    Element ToElement(TRecord record, int index)
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

        var isSelected = SelectedRecordIndexList.Contains(index);
        
        return new FlexRow(WidthFull, Padding(4,8), CursorDefault)
        {
            label,
            
            isSelected ? 
                Background(Theme.ColorForListViewItemSelectedBackground):
                Hover(Background(Theme.ColorForListViewItemHoverBackground))
        };
    }


    internal record State
    {
        public string SearchText { get; init; }
    }
}