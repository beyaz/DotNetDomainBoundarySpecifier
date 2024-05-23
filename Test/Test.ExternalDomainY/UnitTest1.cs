using DotNetDomainBoundarySpecifier.Processors;
using FluentAssertions;

namespace DotNetDomainBoundarySpecifier.Tests;

[TestClass]
public class Scenario1
{
    [TestMethod]
    public void Scenario1Test()
    {
        var analyzeMethodInput = new Analyzer.AnalyzeMethodInput()
        {
            AssemblyFileName = "Test.ExternalDomainY.dll",
            TypeFullName     = "Test.ExternalDomainY.Scenario1.Process1",
            MethodFullName   = "System.String Test.ExternalDomainY.Scenario1.Process1::Method1(System.String,System.Int32,System.Nullable`1<System.DateTime>)"
        };
        
        var records = Analyzer.AnalyzeMethod(new(), analyzeMethodInput);

        records.Count.Should().Be(0);

        var generationOutput = Analyzer.GenerateCode(new(), analyzeMethodInput, records);

        var expected =
            """
           namespace _Contracts_.Test.ExternalDomainY.Scenario1.Process1.Method1;
           
           public sealed class Method1Input : IBankingProxyInput<string>
           {
               public string Parameter1 { get; set; }
               public int Parameter2 { get; set; }
               public DateTime? Parameter3 { get; set; }
           }
           """;
        generationOutput.ContractFile.Content.Trim().Should().BeEquivalentTo(expected.Trim());
    }
}