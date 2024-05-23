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

           """;
        generationOutput.ContractFile.Content.Trim().Should().BeEquivalentTo(expected);
    }
}