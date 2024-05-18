namespace ApiInspector.WebUI.Components;

sealed class AssemblySelector : Component
{
    protected override Element render()
    {
        return new FlexColumn(AlignItemsCenter,Gap(4))
        {
            Directory.GetFiles(Config.AssemblySearchDirectory, "*.dll").Where(x=>!isInDomain(x)).Select(ToElement)
        };
    }

    Element ToElement(string file)
    {
        var fileName = Path.GetFileName(file);

        return new FlexRowCentered(Border(1, solid, Theme.BorderColor), BorderRadius(4), Padding(4,8), CursorDefault, Hover(Border(1,solid,"blue")))
        {
            fileName
        };
    }
}