using Test.ExternalDomainY.Scenario3;

namespace Test.DomainX;

public class Scenario3Test
{
    public static void Scenario3_1()
    {
        var process = new ExternalDomainY.Scenario3.Process3();

        process.MethodB(new Method2In()
        {
            Property0 = "a",
            Property1 = 1,
            Property2 = null,
            Property3 = 2,
            Property5 = new ()
            {
                X ="a",
                Y = 6,
                PropertyNestedUsage2 = new ()
                {
                    Z = DateTime.MaxValue
                }
            }
        });
    }
    
    public static void Scenario3_2()
    {
        var process = new ExternalDomainY.Scenario3.Process3();

        var output = process.MethodC(new Method2In()
        {
            Property0 = "a",
            Property1 = 1,
            Property2 = null,
            Property3 = 2,
            Property5 = new ()
            {
                X ="a",
                Y = 6,
                PropertyNestedUsage2 = new ()
                {
                    Z = DateTime.MaxValue
                }
            }
        });

        output.Value.OutputProperty3.ToString();
    }
    
    public static void Scenario3_3()
    {
        var process = new ExternalDomainY.Scenario3.Process3();

        var output = process.MethodE(new Method2In()
        {
            Property0 = "a",
            Property1 = 1,
            Property2 = null,
            Property3 = 2,
            Property5 = new ()
            {
                X ="a",
                Y = 6,
                PropertyNestedUsage2 = new ()
                {
                    Z = DateTime.MaxValue
                }
            }
        }, null);

        output.Value.ToString();
    }
    
    public static void Scenario3_4()
    {
        var process = new ExternalDomainY.Scenario3.Process3();

        var output = process.MethodF(null, new Method2In()
        {
            Property0 = "a",
            Property1 = 1,
            Property2 = null,
            Property3 = 2,
            Property5 = new ()
            {
                X ="a",
                Y = 6,
                PropertyNestedUsage2 = new ()
                {
                    Z = DateTime.MaxValue
                }
            }
        });

        output.Value.OutputProperty1.ToString();
        output.Value.OutputProperty3.ToString();
    }
}