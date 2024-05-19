using ReactWithDotNet.ThirdPartyLibraries._react_split_;

namespace ApiInspector.WebUI.Components;

sealed class SplitRow : Component
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
                new CssClass("gutter.gutter-horizontal",
                [
                    BackgroundImage("url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAeCAYAAADkftS9AAAAIklEQVQoU2M4c+bMfxAGAgYYmwGrIIiDjrELjpo5aiZeMwF+yNnOs5KSvgAAAABJRU5ErkJggg==')")
                ])
            },

            new Split
            {
                sizes      = sizes,
                gutterSize = 12,
                style      = { SizeFull, DisplayFlexRow },
                direction = "horizontal",
                children =
                {
                    children
                }
            }
        };
    }
}