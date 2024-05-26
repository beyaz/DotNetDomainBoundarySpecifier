namespace Test.ExternalDomainY;

public class AnyProcess
{
    public string Method1(string parameter1, int parameter2, DateTime? parameter3)
    {
        return default;
    }
    
    public string Method2( A parameter3, string parameter1,int parameter2)
    {
        return default;
    }
    
    public AnyOutput Method3(A parameter3)
    {
        return default;
    }
    
    public GenericResponse<AnyOutput> Method4(A parameter3)
    {
        return default;
    }
    
    public GenericResponse<int> Method5(A parameter3)
    {
        return default;
    }
    
    public GenericResponse<int> Method6(A parameter3, ObjectHelper objectHelper)
    {
        return default;
    }
    
    public GenericResponse<AnyOutput> Method7(ObjectHelper objectHelper, A parameter3)
    {
        return default;
    }
    
    public GenericResponse<List<AnyOutput>> Method8(ObjectHelper objectHelper, int accountNumber)
    {
        return default;
    }
}


public class A
{
    public string Property0 { get; set; }
    public int Property1 { get; set; }   
    public DateTime? Property2 { get; set; }
    public long Property3 { get; set; }
    
    public long Property4 { get; set; }
    
    public B Property5 { get; set; }
}


public class B
{
    public string X { get; set; }
    public int Y { get; set; }   
    public DateTime? Z { get; set; }
    
    public C Nested { get; set; }
}

public class C
{
    public string X { get; set; }
    public int Y { get; set; }   
    public DateTime? Z { get; set; }
}

public class AnyOutput
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

