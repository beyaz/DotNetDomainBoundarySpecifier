using ReactWithDotNet.ThirdPartyLibraries._react_split_;

namespace ApiInspector.WebUI.Components;

sealed class SplitColumn : Component
{
    public int[] sizes { get; init; } = [50, 50];

    protected override Element render()
    {
        return new FlexRow(SizeFull)
        {
            new style
            {
                new CssClass("gutter",
                [
                    PaddingTopBottom(8),
                    BackgroundRepeatNoRepeat,
                    BackgroundPosition("50%")
                ]),
                new CssClass("gutter.gutter-vertical",
                [
                    BackgroundImage("url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAB4AAAAFAQMAAABo7865AAAABlBMVEVHcEzMzMzyAv2sAAAAAXRSTlMAQObYZgAAABBJREFUeF5jOAMEEAIEEFwAn3kMwcB6I2AAAAAASUVORK5CYII=')"),
                    //Cursor("col-resize")
                ])
            },

            new Split
            {
                sizes      = sizes,
                gutterSize = 12,
                style      = { SizeFull, DisplayFlexColumn },
                direction = "vertical",
                children =
                {
                    children
                }
            }
        };
    }
}