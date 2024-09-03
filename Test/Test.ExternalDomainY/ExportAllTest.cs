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
            if (externalDomainAssemblyFile != "BOA.Process.Kernel.CoreBanking.Customer.dll")
            {
                continue;
            }

            foreach (var methodDefinition in Analyzer.GetCalledMethodsFromExternalDomain(scope, externalDomainAssemblyFile))
            {
                if (methodDefinition.Name != "GenerateIndividualPersonAndCustomer")
                {
                    continue;
                }

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


    [TestMethod]
    public void ExportSpecificMethod()
    {
        var scope = DefaultScope;

        //var analyzeMethodInput = new Analyzer.AnalyzeMethodInput
        //{
        //    AssemblyFileName = "BOA.Process.Kernel.CoreBanking.Customer.dll",
        //    TypeFullName     = "BOA.Process.Kernel.CoreBanking.Customer.Customer",
        //    MethodFullName   = "GenerateIndividualPersonAndCustomer"
        //};

        var analyzeMethodInput = new Analyzer.AnalyzeMethodInput
        {
            AssemblyFileName = "BOA.Process.Kernel.CustomerGeneral.dll",
            TypeFullName     = "BOA.Process.Kernel.CustomerGeneral.Customer",
            MethodFullName   = "CheckCustomerStateInfo"
        };

        var methodBoundary = Analyzer.AnalyzeMethod(scope, analyzeMethodInput);

        var generationOutput = Analyzer.GenerateCode(scope, analyzeMethodInput, methodBoundary);
        if (generationOutput.Success)
        {
            FileExporter.ExportToFile(scope, generationOutput.Value).Unwrap();
        }
    }
}