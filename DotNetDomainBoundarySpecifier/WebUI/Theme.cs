namespace DotNetDomainBoundarySpecifier.WebUI;

static class Theme
{
    public static string BluePrimary => "#1976d2";
    
    public const string BorderColor = "#d5d5d8";

    public static Style InputStyle =>
    [
        WidthFull,
        Border(1,solid,"rgb(209, 213, 219)"),
        BorderRadius(6),
        Padding(10)

    ];

    public static Element SearchIcon => new svg( Fill(none), svg.Size(14))
    {
        new path {  d = "M2.676 11.027a6.02 6.02 0 0 0 7.157-.347l3.144 3.144a.595.595 0 0 0 .847 0 .6.6 0 0 0 0-.848l-3.143-3.143a6.02 6.02 0 1 0-8.005 1.194Zm.68-9.011a4.797 4.797 0 1 1 5.33 7.977 4.797 4.797 0 0 1-5.33-7.977Z", fill = "currentColor" }
    };
    
    public static StyleModifier Border => Border(1, solid, BorderColor);

    public static int BorderRadius => 6;
    
    public static StyleModifier BackgroundForWindow => Background(rgba(255, 255, 255, 0.4));
    
    public static StyleModifier BackgroundForBrowser => Background("#eff3f8");

    public static StyleModifier BoxShadowForWindow => BoxShadow(0, 30, 30, 0, rgba(69, 42, 124, 0.15));

    public static string ColorForListViewItemSelectedBackground => rgb(163, 239, 243);
    
    public static string ColorForListViewItemHoverBackground => "#f3f4f6";
    
}