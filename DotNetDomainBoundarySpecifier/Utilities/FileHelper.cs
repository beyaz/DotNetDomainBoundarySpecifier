namespace DotNetDomainBoundarySpecifier.Utilities;

static class FileHelper
{
    public static bool IsFileContentAlreadySame(string filePath, string fileContent)
    {
        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath) == fileContent;
        }

        return false;
    }

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
                if (IsFileContentAlreadySame(filePath, fileContent))
                {
                    return default;
                }

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