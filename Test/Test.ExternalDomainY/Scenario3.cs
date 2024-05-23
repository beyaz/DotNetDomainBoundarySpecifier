namespace Test.ExternalDomainY.Scenario3;



public class Process3
{
    public Method2Output MethodB(Method2In parameter3)
    {
        return default;
    }
    
    public GenericResponse<Method2Output> MethodC(Method2In parameter3)
    {
        return default;
    }
    
    public GenericResponse<int> MethodD(Method2In parameter3)
    {
        return default;
    }
    
    public GenericResponse<int> MethodE(Method2In parameter3, ObjectHelper objectHelper)
    {
        return default;
    }
    
    public GenericResponse<Method2Output> MethodF(ObjectHelper objectHelper, Method2In parameter3)
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

public class Method2Output
{
    public string OutputProperty0 { get; set; }
    public int OutputProperty1 { get; set; }   
    public DateTime? OutputProperty2 { get; set; }
    public long OutputProperty3 { get; set; }
    
}


public class GenericResponse<T>
{
    public T Value { get; set; }
}

public class ObjectHelper
{
    
}

