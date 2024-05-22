using Newtonsoft.Json;

namespace DotNetDomainBoundarySpecifier;

static class ConfigReader
{
    static ConfigInfo cache;

    public static ConfigInfo ReadConfig()
    {
        return cache ??= ReadFromFileSystem();
    }

    static ConfigInfo ReadFromFileSystem()
    {
        var appFolder = Path.GetDirectoryName(typeof(ConfigReader).Assembly.Location);
        if (appFolder == null)
        {
            throw new ArgumentException("assembly location not found");
        }

        var config = JsonConvert.DeserializeObject<ConfigInfo>(File.ReadAllText(Path.Combine(appFolder, "DotNetDomainBoundarySpecifier.Config.json")));

        var isRunningInVisualStudio = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VisualStudioEdition"));
        if (isRunningInVisualStudio)
        {
            config = config with { UseUrls = false };
        }

        return config;
    }
}