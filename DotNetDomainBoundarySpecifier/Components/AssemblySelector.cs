namespace ApiInspector.WebUI.Components;

sealed class AssemblySelector : Component
{
    protected override Element render()
    {
        return new ItemList<string>
        {
            Records = Directory.GetFiles(Config.AssemblySearchDirectory, "*.dll").Where(x => !isInDomain(x)).ToList(),
        };
    }

}