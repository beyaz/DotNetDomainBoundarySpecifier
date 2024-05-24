namespace DotNetDomainBoundarySpecifier.WebUI;

static class Theme
{
    public const string BorderColor = "#d5d5d8";

    public static StyleModifier BackgroundForBrowser => Background("#eff3f8");

    public static StyleModifier BackgroundForWindow => Background(rgba(255, 255, 255, 0.4));
    public static string BluePrimary => "#1976d2";

    public static StyleModifier Border => Border(1, solid, BorderColor);

    public static int BorderRadius => 6;

    public static StyleModifier BoxShadowForWindow => BoxShadow(0, 30, 30, 0, rgba(69, 42, 124, 0.15));

    public static Style InputStyle =>
    [
        WidthFull,
        Border(1, solid, "rgb(209, 213, 219)"),
        BorderRadius(6),
        Padding(10)
    ];

    public static Element SearchIcon => new svg(ViewBox(0, 0, 16, 16), Fill("currentColor"), svg.FocusableFalse,  Size(16))
    {
        new path { d = "M6.5 1a5.5 5.5 0 110 11 5.5 5.5 0 110-11zm0 1a4.5 4.5 0 100 9 4.5 4.5 0 000-9z" },
        new path { d = "M9.646 9.646a.5.5 0 01.638-.058l.069.058 4.5 4.5a.5.5 0 01-.638.765l-.069-.058-4.5-4.5a.5.5 0 010-.707z" }
    };

    public static class ListView
    {
        public static string ItemHoverBackgroundColor => "#f3f4f6";

        public static string ItemSelectedBackgroundColor => rgb(163, 239, 243);

        public static string MarkedItemBackgroundColor => "#f5f7e6";
    }
}