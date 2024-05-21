using ReactWithDotNet.ThirdPartyLibraries._react_split_;

namespace DotNetDomainBoundarySpecifier.WebUI.Components;

sealed class SplitColumn : Component
{
    public int[] sizes { get; init; }

    protected override Element render()
    {
        return new div(SizeFull)
        {
            new style
            {
                new CssClass("gutter",
                [
                    BackgroundRepeatNoRepeat,
                    BackgroundPosition("50%")
                ]),
                new CssClass("gutter.gutter-vertical",
                [
                    BackgroundImage("url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAB4AAAAFAQMAAABo7865AAAABlBMVEVHcEzMzMzyAv2sAAAAAXRSTlMAQObYZgAAABBJREFUeF5jOAMEEAIEEFwAn3kMwcB6I2AAAAAASUVORK5CYII=')")
                ])
            },

            new Split
            {
                sizes      = sizes,
                gutterSize = 12,
                style      = { SizeFull, DisplayFlexColumn },
                direction  = "vertical",
                children =
                {
                    children
                }
            }
        };
    }
}