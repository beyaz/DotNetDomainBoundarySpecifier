using Test.ExternalDomainY.Scenario2;

namespace Test.DomainX
{
    public class Class1
    {
        public static void MethodA()
        {
            var process = new Test.ExternalDomainY.Scenario2.Process2();

            process.MethodA(new ()
            {
                Property0 = "a",
                Property1 = 1,
                Property2 = null,
                Property3 = 2,
                Property5 = new ()
                {
                    X="a",
                    Y = 6,
                    PropertyNestedUsage2 = new ()
                    {
                        Z = DateTime.MaxValue
                    }
                }
            }, "A", 5);
        }
    }
}
