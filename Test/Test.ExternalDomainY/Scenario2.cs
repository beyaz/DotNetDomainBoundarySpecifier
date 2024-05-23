namespace Test.ExternalDomainY.Scenario2;

public class Process2
{
    public string MethodA( Method2In parameter3, string parameter1,int parameter2)
    {
        return default;
    }
}


public class Method2In
{
    public string Property0 { get; set; }
    public int Property1 { get; set; }   
    public DateTime? Property2 { get; set; }
    public long Property3 { get; set; }
    
    public long Property4 { get; set; }
    
    public NestedUsageClass Property5 { get; set; }
}


public class NestedUsageClass
{
    public string X { get; set; }
    public int Y { get; set; }   
    public DateTime? Z { get; set; }
    
    public NestedUsageClass2 PropertyNestedUsage2 { get; set; }
}

public class NestedUsageClass2
{
    public string X { get; set; }
    public int Y { get; set; }   
    public DateTime? Z { get; set; }
}


