using Test.ExternalDomainY;

//using Output = System.Collections.Generic.List<AnyProcessCaller>;

namespace Test.DomainX;

public class AnyProcessCaller
{
    public static void Call_Method1()
    {
        var process = new AnyProcess();

        process.Method1(default,default,default);
    }
    
    public static void Call_Method2()
    {
        var process = new AnyProcess();

        process.Method2(new ()
        {
            Property0 = "a",
            Property1 = 1,
            Property2 = null,
            Property3 = 2,
            Property5 = new ()
            {
                X ="a",
                Y = 6,
                Nested = new ()
                {
                    Z = DateTime.MaxValue
                }
            }
        }, "A", 5);
    }
    
    public static void Call_Method3()
    {
        var process = new AnyProcess();

        process.Method3(new ()
        {
            Property0 = "a",
            Property1 = 1,
            Property2 = null,
            Property3 = 2,
            Property5 = new ()
            {
                X ="a",
                Y = 6,
                Nested = new ()
                {
                    Z = DateTime.MaxValue
                }
            }
        });
    }
    
    public static void Call_Method4()
    {
        var process = new AnyProcess();

        var output = process.Method4(new ()
        {
            Property0 = "a",
            Property1 = 1,
            Property2 = null,
            Property3 = 2,
            Property5 = new ()
            {
                X ="a",
                Y = 6,
                Nested = new ()
                {
                    Z = DateTime.MaxValue
                }
            }
        });

        Dummy(output.Value.OutputProperty3);
    }
    
    public static void Call_Method5()
    {
        var process = new AnyProcess();

        process.Method5(new ()
        {
            Property0 = "a",
            Property1 = 1,
            Property2 = null,
            Property3 = 2,
            Property5 = new ()
            {
                X ="a",
                Y = 6,
                Nested = new ()
                {
                    Z = DateTime.MaxValue
                }
            }
        });

    }
    
    public static void Call_Method6()
    {
        var process = new AnyProcess();

        process.Method6(new ()
        {
            Property0 = "a",
            Property1 = 1,
            Property2 = null,
            Property3 = 2,
            Property5 = new ()
            {
                X ="a",
                Y = 6,
                Nested = new ()
                {
                    Z = DateTime.MaxValue
                }
            }
        },default);

    }
    
    public static void Call_Method7()
    {
        var process = new AnyProcess();

        var output = process.Method7(default, new ()
        {
            Property0 = "a",
            Property1 = 1,
            Property2 = null,
            Property3 = 2,
            Property5 = new ()
            {
                X ="a",
                Y = 6,
                Nested = new ()
                {
                    Z = DateTime.MaxValue
                }
            }
        });

        Dummy(output.Value.OutputProperty1);
        Dummy(output.Value.OutputProperty3);
    }
    
    public static void Call_Method8()
    {
        var process = new AnyProcess();

        var output = process.Method8(default, default);

        Dummy(output.Value[0].OutputProperty1);
        Dummy(output.Value[1].OutputProperty3);
    }

    static void Dummy(object _)
    {
        
    }
}