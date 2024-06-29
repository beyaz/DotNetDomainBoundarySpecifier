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
            foreach (var methodDefinition in Analyzer.GetCalledMethodsFromExternalDomain(scope, externalDomainAssemblyFile))
            {
                //if (methodDefinition.Name != "GetCityList")
                //{
                //    continue;
                //}


                var analyzeMethodInput = new Analyzer.AnalyzeMethodInput
                {
                    AssemblyFileName = externalDomainAssemblyFile,
                    TypeFullName     = methodDefinition.DeclaringType.FullName,
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