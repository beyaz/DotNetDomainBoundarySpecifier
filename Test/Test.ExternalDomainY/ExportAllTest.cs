namespace DotNetDomainBoundarySpecifier.Tests;

[TestClass]
public class ExportAllTest
{
    [TestMethod]
    public void ExportAll()
    {
        var scope = DefaultScope;

        foreach (var externalDomainAssemblyFile in scope.Config.UsedExternalAssemblies)
        {
            foreach (var typeDefinition in scope.GetTypesInAssemblyFile(externalDomainAssemblyFile))
            {
                foreach (var methodDefinition in Analyzer.GetCalledMethodsFromExternalDomain(scope, externalDomainAssemblyFile))
                {
                    var analyzeMethodInput = new Analyzer.AnalyzeMethodInput
                    {
                        AssemblyFileName = externalDomainAssemblyFile,
                        TypeFullName     = typeDefinition.FullName,
                        MethodFullName   = methodDefinition.FullName
                    };

                    var methodBoundary = Analyzer.AnalyzeMethod(scope, analyzeMethodInput);

                    var generationOutput = Analyzer.GenerateCode(scope, analyzeMethodInput, methodBoundary);
                    if (generationOutput.Success)
                    {
                        FileExporter.ExportToFile(scope, generationOutput.Value).Unwrap();
                    }
                  
                }
            }
        }
    }
}