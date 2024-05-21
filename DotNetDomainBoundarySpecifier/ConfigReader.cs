using Newtonsoft.Json;

namespace DotNetDomainBoundarySpecifier;

static class ConfigReader
{
    public static ConfigInfo ReadConfig()
    {
        var appFolder = Path.GetDirectoryName(typeof(ConfigReader).Assembly.Location);
        if (appFolder == null)
        {
            throw new ArgumentException("assembly location not found");
        }

        var config = JsonConvert.DeserializeObject<ConfigInfo>(File.ReadAllText(Path.Combine(appFolder, "DotNetDomainBoundarySpecifier.Config.json")));

        var isRunningInVisiualStudio = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VisualStudioEdition"));
        if (isRunningInVisiualStudio)
        {
            config = config with { UseUrls = false };
        }

        return config;
    }
}