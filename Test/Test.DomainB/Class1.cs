namespace Test.DomainB;

public class Process1
{
    public string Method1(string parameter1)
    {
        return default;
    }
    
    public string Method1(string parameter1,int parameter2)
    {
        return default;
    }
    
    public string Method1(string parameter1,int parameter2, DateTime? parameter3)
    {
        return default;
    }
}

public class Process2
{
   
    public string Method1(string parameter1,int parameter2, DateTime? parameter3)
    {
        return default;
    }
    
    public string Method2(string parameter1,int parameter2, Method2Input parameter3)
    {
        return default;
    }
    
    public Method2Output Method2(Method2Input parameter3)
    {
        return default;
    }
}


public class Method2Input
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

public class Method2Output
{
    public string OutputProperty0 { get; set; }
    public int OutputProperty1 { get; set; }   
    public DateTime? OutputProperty2 { get; set; }
    public long OutputProperty3 { get; set; }
    
}

