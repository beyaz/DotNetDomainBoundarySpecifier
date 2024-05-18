namespace ApiInspector.WebUI;

static class Theme
{
    public const string BorderColor = "#d5d5d8";
    public static string BackgroundColor = "#eff3f8";
    public static string WindowBackgroundColor = rgba(255, 255, 255, 0.4);

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
    
    public static class Border
    {
        public static StyleModifier Component => Border(1, solid, BorderColor);
    }
    
    public static class MainWindow
    {
        public static StyleModifier Background => Background(WindowBackgroundColor);
        
    }
}