using DotNetDomainBoundarySpecifier.Processors;
using FluentAssertions;

namespace DotNetDomainBoundarySpecifier.Tests;

[TestClass]
public class Scenario2
{
    [TestMethod]
    public void Scenario2Test()
    {
        var analyzeMethodInput = new Analyzer.AnalyzeMethodInput()
        {
            AssemblyFileName = "Test.ExternalDomainY.dll",
            TypeFullName     = "Test.ExternalDomainY.Scenario2.Process2",
            MethodFullName   = "System.String Test.ExternalDomainY.Scenario2.Process2::MethodA(Test.ExternalDomainY.Scenario2.Method2Input,System.String,System.Int32)"
        };
        
        
        var records = Analyzer.AnalyzeMethod(new(), analyzeMethodInput);

        records.Count.Should().Be(9);

        var generationOutput = Analyzer.GenerateCode(new(), analyzeMethodInput, records);

        var expected =
            """
           namespace BOA.Card.Contracts.Banking.Test.ExternalDomainY.Scenario2.Process2.MethodA;
           
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