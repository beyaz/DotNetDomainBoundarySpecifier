﻿namespace ApiInspector.WebUI.Components;

public delegate Task ListViewSelectedItemsChanged<in TRecord>(IReadOnlyList<TRecord> selectedItems);

sealed class ListView<TRecord> : Component<ListView<TRecord>.State>
{
    public required IReadOnlyList<TRecord> ItemsSource { get; init; } = [];

    public IReadOnlyList<TRecord> SelectedItems { get; init; } = [];

    [ReactCustomEvent]
    public ListViewSelectedItemsChanged<TRecord> SelectedItemsChanged { get; init; }

    public bool SelectionIsSingle { get; init; }

    protected override Task constructor()
    {
        state = new()
        {
            SelectedItems             = SelectedItems,
            SelectedItemsInitialValue = SelectedItems,
            SelectionIsSingle         = SelectionIsSingle
        };

        return Task.CompletedTask;
    }

    protected override Task OverrideStateFromPropsBeforeRender()
    {
        if (!state.SelectedItemsInitialValue.SequenceEqual(SelectedItems))
        {
            state = state with
            {
                SelectedItems = SelectedItems,
                SelectedItemsInitialValue = SelectedItems,
                SelectionIsSingle = SelectionIsSingle
            };
        }

        return Task.CompletedTask;
    }

    protected override Element render()
    {
        return new FlexColumn(BorderRadius(Theme.BorderRadius), Theme.Border)
        {
            new FlexRowCentered(Padding(8, 16), Background("#f9fafb"), BorderRadius(Theme.BorderRadius, Theme.BorderRadius, 0, 0))
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
                    BorderBottom(1, solid, Theme.BorderColor)
                }
            },

            new FlexColumn(AlignItemsCenter, PaddingTopBottom(4), Gap(8), Background("white"), BorderBottomLeftRadius(Theme.BorderRadius), BorderBottomRightRadius(Theme.BorderRadius))
            {
                ItemsSource.Select(ToElement)
            }
        };
    }

    Task OnItemClicked(MouseEvent e)
    {
        if (int.TryParse(e.currentTarget.id, out var index))
        {
            if (SelectionIsSingle)
            {
                state = state with
                {
                    SelectedItems = [ItemsSource[index]]
                };
            }
            else
            {
                state = state with
                {
                    SelectedItems = state.SelectedItems.Toggle(ItemsSource[index])
                };
            }

            DispatchEvent(SelectedItemsChanged, [state.SelectedItems]);
        }

        return Task.CompletedTask;
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

        var isSelected = SelectedItems.Contains(record);

        return new FlexRow(Id(index), WidthFull, Padding(4, 8), CursorDefault)
        {
            label,

            isSelected ?
                Background(Theme.ColorForListViewItemSelectedBackground) :
                Hover(Background(Theme.ColorForListViewItemHoverBackground)),

            isSelected ? null : OnClick(OnItemClicked)
        };
    }

    internal record State
    {
        public string SearchText { get; init; }

        public IReadOnlyList<TRecord> SelectedItemsInitialValue { get; init; } = [];

        public bool SelectionIsSingle { get; init; }

        public IReadOnlyList<TRecord> SelectedItems { get; init; } = [];
    }
}