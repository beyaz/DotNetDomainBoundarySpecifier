using DotNetDomainBoundarySpecifier.WebUI.Components;

namespace DotNetDomainBoundarySpecifier.Tests;

[TestClass]
public class ExportAllTest
{
    [TestMethod]
    public void ExportAll()
    {
        foreach (var externalDomainAssemblyFile in AssemblySelector.ExternalDomainAssemblyFiles)
        {
            foreach (var typeDefinition in DefaultScope.GetTypesInAssemblyFile(externalDomainAssemblyFile))
            {
                foreach (var methodDefinition in Analyzer.GetCalledMethodsFromExternalDomain(DefaultScope, externalDomainAssemblyFile))
                {
                    var analyzeMethodInput = new Analyzer.AnalyzeMethodInput
                    {
                        AssemblyFileName = externalDomainAssemblyFile,
                        TypeFullName     = typeDefinition.FullName,
                        MethodFullName   = methodDefinition.FullName
                    };

                    var methodBoundary = Analyzer.AnalyzeMethod(DefaultScope, analyzeMethodInput);

                    var generationOutput = Analyzer.GenerateCode(DefaultScope, analyzeMethodInput, methodBoundary).Unwrap();

                    FileExporter.ExportToFile(DefaultScope, generationOutput).Unwrap();
                }
            }
        }
    }
}