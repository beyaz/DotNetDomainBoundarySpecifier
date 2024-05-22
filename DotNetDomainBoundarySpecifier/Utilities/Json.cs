using System.Text.Json;

namespace DotNetDomainBoundarySpecifier.Utilities;

static class Json
{
    public static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json);
    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value);
}