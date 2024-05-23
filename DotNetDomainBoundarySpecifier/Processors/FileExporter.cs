namespace DotNetDomainBoundarySpecifier.Processors;

static class FileExporter
{
    public static Unit ExportToFile(Scope scope, CodeGenerationOutput output)
    {
        var config = scope.Config;

        return Run([
            () => writeToFile(config.ExportDirectoryForTypes, output.ContractFile),
            () => writeToFile(config.ExportDirectoryForProcess, output.ProcessFile)
        ]);

        static Exception writeToFile(string directory, FileModel fileModel)
        {
            return WriteCSharpFile($"{directory}{fileModel.Name}.cs", fileModel.Content);
        }
    }
}