namespace DotNetDomainBoundarySpecifier.Utilities;

static class FileHelper
{
    public static Exception WriteCSharpFile(string filePath, string fileContent)
    {
        try
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
        catch (Exception exception)
        {
            return exception;
        }

        return default;
    }
}