
namespace DotNetDomainBoundarySpecifier.Tests;

[TestClass]
public class GenerationTest
{
    [TestMethod]
    public void Method1()
    {
        var analyzeMethodInput = new Analyzer.AnalyzeMethodInput()
        {
            AssemblyFileName = "Test.ExternalDomainY.dll",
            TypeFullName     = "Test.ExternalDomainY.AnyProcess",
            MethodFullName   = "System.String Test.ExternalDomainY.AnyProcess::Method1(System.String,System.Int32,System.Nullable`1<System.DateTime>)"
        };
        
        var records = Analyzer.AnalyzeMethod(DefaultScope, analyzeMethodInput);

        records.Count.Should().Be(0);

        var generationOutput = Analyzer.GenerateCode(DefaultScope, analyzeMethodInput, records);

        var expected =
            """
           namespace _Contracts_.Test.ExternalDomainY.AnyProcess.Method1;
           
           public sealed class Method1Input : IBankingProxyInput<string>
           {
               public string Parameter1 { get; set; }
               public int Parameter2 { get; set; }
               public DateTime? Parameter3 { get; set; }
           }
           """;
        generationOutput.ContractFile.Content.Trim().Should().BeEquivalentTo(expected.Trim());
    }
    
    
    [TestMethod]
    public void Method2()
    {
        var analyzeMethodInput = new Analyzer.AnalyzeMethodInput()
        {
            AssemblyFileName = "Test.ExternalDomainY.dll",
            TypeFullName     = "Test.ExternalDomainY.AnyProcess",
            MethodFullName   = "System.String Test.ExternalDomainY.AnyProcess::Method2(Test.ExternalDomainY.A,System.String,System.Int32)"
        };
        
        var records = Analyzer.AnalyzeMethod(DefaultScope, analyzeMethodInput);

        records.Count.Should().Be(9);

        var generationOutput = Analyzer.GenerateCode(DefaultScope, analyzeMethodInput, records);

        var expected =
            """
            namespace _Contracts_.Test.ExternalDomainY.AnyProcess.Method2;
            
            public sealed class Method2Input : IBankingProxyInput<string>
            {
                public A Parameter3 { get; set; }
                public string Parameter1 { get; set; }
                public int Parameter2 { get; set; }
            }
            
            public sealed class A
            {
                public string Property0 { get; set; }
                public int Property1 { get; set; }
                public DateTime? Property2 { get; set; }
                public long Property3 { get; set; }
                public B Property5 { get; set; }
            }
            
            public sealed class B
            {
                public string X { get; set; }
                public int Y { get; set; }
                public C Nested { get; set; }
            }
            
            public sealed class C
            {
                public DateTime? Z { get; set; }
            }
            """;
        generationOutput.ContractFile.Content.Trim().Should().BeEquivalentTo(expected.Trim());
    }
}