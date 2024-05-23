namespace Test.DomainX;

public class Scenario1
{
    public static void AnyMethod()
    {
        var process = new ExternalDomainY.Scenario1.Process1();

        process.Method1(default,default,default);
    }
}