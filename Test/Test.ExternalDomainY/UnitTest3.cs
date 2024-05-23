namespace DotNetDomainBoundarySpecifier.Tests;

[TestClass]
public class Scenario3
{
    [TestMethod]
    public void Scenario3Test()
    {
        var analyzeMethodInput = new Analyzer.AnalyzeMethodInput()
        {
            AssemblyFileName = "Test.ExternalDomainY.dll",
            TypeFullName     = "Test.ExternalDomainY.Scenario3.Process3",
            MethodFullName   = "Test.ExternalDomainY.Scenario3.Method2Output Test.ExternalDomainY.Scenario3.Process3::MethodB(Test.ExternalDomainY.Scenario3.Method2Input)"
        };
        
        
        var records = Analyzer.AnalyzeMethod(DefaultScope, analyzeMethodInput);

        records.Count.Should().Be(11);

        var generationOutput = Analyzer.GenerateCode(DefaultScope, analyzeMethodInput, records);

        var expected =
            """
           namespace _Contracts_.Test.ExternalDomainY.Scenario2.Process2.MethodA;
           
           public sealed class MethodAInput : IBankingProxyInput<string>
           {
               public Method2Input Parameter3 { get; set; }
               public string Parameter1 { get; set; }
               public int Parameter2 { get; set; }
           }
           """;
        generationOutput.ContractFile.Content.Trim().Should().BeEquivalentTo(expected.Trim());
    }
}