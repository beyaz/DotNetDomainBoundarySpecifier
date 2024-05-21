using System.Text;

namespace DotNetDependencyExtractor;

static class FileHelper
{
    public static void WriteCSharpFile(string filePath, string fileContent)
    {
        if (filePath == null)
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        var directoryName = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directoryName) &&
            !Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        File.WriteAllText(filePath, fileContent, Encoding.UTF8);
    }
}