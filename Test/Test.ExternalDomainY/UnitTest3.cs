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
            MethodFullName   = "Test.ExternalDomainY.Scenario3.Method2Output Test.ExternalDomainY.Scenario3.Process3::MethodB(Test.ExternalDomainY.Scenario3.Method2In)"
        };
        
        
        var records = Analyzer.AnalyzeMethod(DefaultScope, analyzeMethodInput);

        records.Count.Should().Be(11);

        var generationOutput = Analyzer.GenerateCode(DefaultScope, analyzeMethodInput, records);

        var expected =
            """
           namespace _Contracts_.Test.ExternalDomainY.Scenario3.Process3.MethodB;
           
           public sealed class MethodBInput : IBankingProxyInput<Output>
           {
               public string Property0 { get; set; }
               public int Property1 { get; set; }
               public DateTime? Property2 { get; set; }
               public long Property3 { get; set; }
               public NestedUsageClass Property5 { get; set; }
           }
           public sealed class NestedUsageClass
           {
               public string X { get; set; }
               public int Y { get; set; }
               public NestedUsageClass2 PropertyNestedUsage2 { get; set; }
           }
           public sealed class NestedUsageClass2
           {
               public DateTime? Z { get; set; }
           }
           public sealed class Method2Output
           {
               public int OutputProperty1 { get; set; }
               public long OutputProperty3 { get; set; }
           }
           """;
        generationOutput.ContractFile.Content.Trim().Should().BeEquivalentTo(expected.Trim());
    }
}