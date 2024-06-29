using DotNetDomainBoundarySpecifier.WebUI.Components;

namespace DotNetDomainBoundarySpecifier.Tests;

[TestClass]
public class ExportAllTest
{
    [TestMethod]
    public void ExportAll()
    {
        var scope = DefaultScope;

        foreach (var externalDomainAssemblyFile in AssemblySelector.ExternalDomainAssemblyFiles)
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

                    var generationOutput = Analyzer.GenerateCode(scope, analyzeMethodInput, methodBoundary).Unwrap();

                    FileExporter.ExportToFile(scope, generationOutput).Unwrap();
                }
            }
        }
    }
}