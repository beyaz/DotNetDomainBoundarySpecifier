
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
           
           using Output = string;
           
           public sealed class Method1Input : IBankingProxyInput<Output>
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
            
            using Output = string;
            
            public sealed class Method2Input : IBankingProxyInput<Output>
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
    
    
    [TestMethod]
    public void Method3()
    {
        var analyzeMethodInput = new Analyzer.AnalyzeMethodInput()
        {
            AssemblyFileName = "Test.ExternalDomainY.dll",
            TypeFullName     = "Test.ExternalDomainY.AnyProcess",
            MethodFullName   = "Test.ExternalDomainY.AnyOutput Test.ExternalDomainY.AnyProcess::Method3(Test.ExternalDomainY.A)"
        };
        
        var records = Analyzer.AnalyzeMethod(DefaultScope, analyzeMethodInput);

        records.Count.Should().Be(11);

        var generationOutput = Analyzer.GenerateCode(DefaultScope, analyzeMethodInput, records);

        var expected =
            """
            namespace _Contracts_.Test.ExternalDomainY.AnyProcess.Method3;
            
            using Output = AnyOutput;
            
            public sealed class Method3Input : IBankingProxyInput<Output>
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
            
            public sealed class AnyOutput
            {
                public int OutputProperty1 { get; set; }
                public long OutputProperty3 { get; set; }
            }
            """;
        generationOutput.ContractFile.Content.Trim().Should().BeEquivalentTo(expected.Trim());
    }
    
    
    [TestMethod]
    public void Method4()
    {
        var analyzeMethodInput = new Analyzer.AnalyzeMethodInput()
        {
            AssemblyFileName = "Test.ExternalDomainY.dll",
            TypeFullName     = "Test.ExternalDomainY.AnyProcess",
            MethodFullName   = "Test.ExternalDomainY.GenericResponse`1<Test.ExternalDomainY.AnyOutput> Test.ExternalDomainY.AnyProcess::Method4(Test.ExternalDomainY.A)"
        };
        
        var records = Analyzer.AnalyzeMethod(DefaultScope, analyzeMethodInput);

        records.Count.Should().Be(11);

        var generationOutput = Analyzer.GenerateCode(DefaultScope, analyzeMethodInput, records);

        var expected =
            """
            namespace _Contracts_.Test.ExternalDomainY.AnyProcess.Method4;

            using Output = AnyOutput;

            public sealed class Method4Input : IBankingProxyInput<Output>
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

            public sealed class AnyOutput
            {
                public int OutputProperty1 { get; set; }
                public long OutputProperty3 { get; set; }
            }
            """;
        generationOutput.ContractFile.Content.Trim().Should().BeEquivalentTo(expected.Trim());
    }
}