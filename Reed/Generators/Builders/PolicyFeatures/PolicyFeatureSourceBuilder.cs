namespace Reed.Generators;

public abstract class PolicyFeatureSourceBuilder
{
    private readonly ResilientMethodSourceBuilder _methodBuilder;
    
    public ResilientMethodSourceBuilder MethodSourceBuilder => _methodBuilder;
    
    internal PolicyFeatureSourceBuilder(ResilientMethodSourceBuilder methodBuilder)
    {
        _methodBuilder = methodBuilder;
    }

    public abstract void BuildFields(CsharpStringBuilder strbldr);
    public abstract void BuildBefore(CsharpStringBuilder strbldr);
    public abstract void BuildAfter(CsharpStringBuilder strbldr);
    
    public abstract int Priority { get; }
}