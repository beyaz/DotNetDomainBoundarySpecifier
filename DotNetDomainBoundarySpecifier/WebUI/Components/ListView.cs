namespace DotNetDomainBoundarySpecifier.WebUI.Components;

public delegate Task ListViewSelectedItemsChanged<in TRecord>(string senderName, IReadOnlyList<TRecord> selectedItems);

public delegate Task ListViewSelectedItemChanged<in TRecord>(string senderName, TRecord selectedItem);

sealed class ListView<TRecord> : Component<ListView<TRecord>.State>
{
    public required IReadOnlyList<TRecord> ItemsSource { get; init; } = [];

    public IReadOnlyList<TRecord> MarkedItems { get; init; } = [];

    public required string Name { get; init; }

    public TRecord SelectedItem { get; init; }

    [CustomEvent]
    public ListViewSelectedItemChanged<TRecord> SelectedItemChanged { get; init; }

    public IReadOnlyList<TRecord> SelectedItems { get; init; } = [];

    [CustomEvent]
    public ListViewSelectedItemsChanged<TRecord> SelectedItemsChanged { get; init; }

    public bool SelectionIsSingle { get; init; }

    public string Title { get; init; }

    protected override Task constructor()
    {
        state = new()
        {
            SelectedItems             = SelectedItems,
            SelectedItemsInitialValue = SelectedItems,
            SelectionIsSingle         = SelectionIsSingle,
            SearchText                = LastUsedSearchText.TryRead(Name)
        };

        return Task.CompletedTask;
    }

    protected override Task OverrideStateFromPropsBeforeRender()
    {
        bool hasChange;

        if (SelectionIsSingle)
        {
            hasChange = SelectedItem is not null ? !SelectedItem.Equals(state.SelectedItem) : state.SelectedItem is not null;
        }
        else
        {
            hasChange = !state.SelectedItemsInitialValue.SequenceEqual(SelectedItems);
        }

        if (hasChange)
        {
            state = state with
            {
                SelectedItem = SelectedItem,
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
            new FlexColumnCentered(Gap(4))
            {
                Title.HasValue() ? (b)Title : null,

                IsInSkeletonMode[Context] ? new Skeleton { Padding(8, 16) } :
                    new SearchTextBox
                    {
                        Value = state.SearchText,
                        OnValueChange = x =>
                        {
                            state = state with { SearchText = x };

                            LastUsedSearchText.Save(Name, x);

                            return Task.CompletedTask;
                        }
                    },
                new Style
                {
                    BorderBottom(1, solid, Theme.BorderColor),
                    BorderRadius(Theme.BorderRadius, Theme.BorderRadius, 0, 0),
                    Padding(8, 16),
                    Background("#f9fafb")
                }
            },

            new FlexColumn(AlignItemsCenter, PaddingTopBottom(4), Gap(8), Background("white"), BorderBottomLeftRadius(Theme.BorderRadius), BorderBottomRightRadius(Theme.BorderRadius))
            {
                OverflowYAuto,
                ItemsSource?.Select(ToElement).OrderDescending(Marker.Comparer)
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
                    SelectedItem = ItemsSource[index]
                };

                DispatchEvent(SelectedItemChanged, [Name, state.SelectedItem]);
            }
            else
            {
                state = state with
                {
                    SelectedItems = state.SelectedItems.Toggle(ItemsSource[index])
                };

                DispatchEvent(SelectedItemsChanged, [Name, state.SelectedItems]);
            }
        }

        return Task.CompletedTask;
    }

    HtmlElement ToElement(TRecord record, int index)
    {
        var label = record.ToString();

        if (label is null)
        {
            return default;
        }

        if (state.SearchText.HasValue())
        {
            if (!label.Contains(state.SearchText, StringComparison.OrdinalIgnoreCase))
            {
                return default;
            }
        }

        bool isSelected;
        if (SelectionIsSingle)
        {
            isSelected = record.Equals(SelectedItem);
        }
        else
        {
            isSelected = SelectedItems.Contains(record);
        }

        var isMarked = MarkedItems.Contains(record);

        return new FlexRow(Id(index), WidthFull, Padding(4, 8), BorderRadius(3), CursorDefault, AlignItemsCenter, WordBreakAll)
        {
            label,

            isSelected ? Background(Theme.ListView.ItemSelectedBackgroundColor) : Hover(Background(Theme.ListView.ItemHoverBackgroundColor)),

            isSelected ? null : OnClick(OnItemClicked),

            isMarked && !isSelected ? Background(Theme.ListView.MarkedItemBackgroundColor) : null,

            isMarked ? Marker.Mark : null
        };
    }

    static class LastUsedSearchText
    {
        public static void Save(string listViewComponentName, string lastUsedSearchText)
        {
            File.WriteAllText(CalculateFilePath(listViewComponentName), lastUsedSearchText, Encoding.UTF8);
        }

        public static string TryRead(string listViewComponentName)
        {
            return Try(() => File.ReadAllText(CalculateFilePath(listViewComponentName))).Value;
        }

        static string CalculateFilePath(string listViewComponentName)
        {
            return Path.GetTempPath()
                   + nameof(DotNetDomainBoundarySpecifier)
                   + ".ListView."
                   + listViewComponentName
                   + ".json";
        }
    }

    static class Marker
    {
        public static readonly IComparer<HtmlElement> Comparer = new MarkedComparer();
        public static HtmlElementModifier Mark => Data("isMarked", 1);

        class MarkedComparer : IComparer<HtmlElement>
        {
            public int Compare(HtmlElement x, HtmlElement y)
            {
                if (x?.data.ContainsKey("isMarked") is true)
                {
                    if (y?.data.ContainsKey("isMarked") is true)
                    {
                        return 0;
                    }

                    return 1;
                }

                return -1;
            }
        }
    }

    internal record State
    {
        public string SearchText { get; init; }

        public IReadOnlyList<TRecord> SelectedItemsInitialValue { get; init; } = [];

        public bool SelectionIsSingle { get; init; }

        public IReadOnlyList<TRecord> SelectedItems { get; init; } = [];

        public TRecord SelectedItem { get; init; }
    }
}