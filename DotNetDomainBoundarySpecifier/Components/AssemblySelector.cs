namespace ApiInspector.WebUI.Components;

sealed class AssemblySelector : Component
{
    protected override Element render()
    {
        return new ListView<string>
        {
            Records = Directory.GetFiles(Config.AssemblySearchDirectory, "*.dll").Where(x => !isInDomain(x)).Select(x=>Path.GetFileName(x)).ToList(),
        };
    }

}