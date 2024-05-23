using Newtonsoft.Json;

namespace DotNetDomainBoundarySpecifier;

static class ConfigReader
{
    static Config cache;

    public static Config ReadConfig()
    {
        return cache ??= ReadFromFileSystem();
    }

    static Config ReadFromFileSystem()
    {
        var appFolder = Path.GetDirectoryName(typeof(ConfigReader).Assembly.Location);
        if (appFolder == null)
        {
            throw new ArgumentException("assembly location not found");
        }

        var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Path.Combine(appFolder, "DotNetDomainBoundarySpecifier.Config.json")));

        var isRunningInVisualStudio = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VisualStudioEdition"));
        if (isRunningInVisualStudio)
        {
            config = config with { UseUrls = false };
        }

        return config;
    }
}