using DotNetDomainBoundarySpecifier.Processors;

namespace DotNetDomainBoundarySpecifier.Tests;

[TestClass]
public class Scenario1
{
    [TestMethod]
    public void Scenario1Test()
    {
        Analyzer.AnalyzeMethod(new(), new()
        {
            AssemblyFileName = "Test.DomainX",
            TypeFullName     = "Test.ExternalDomainY.Scenario1",
            MethodFullName   = "gg"
        });
    }
}