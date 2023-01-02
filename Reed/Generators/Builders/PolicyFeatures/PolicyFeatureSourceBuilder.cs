namespace Reed.Generators;

public abstract class PolicyFeatureSourceBuilder
{
    private readonly ResilientMethodSourceBuilder _methodBuilder;
    
    public ResilientMethodSourceBuilder MethodSourceBuilder => _methodBuilder;
    
    internal PolicyFeatureSourceBuilder(ResilientMethodSourceBuilder methodBuilder)
    {
        _methodBuilder = methodBuilder;
    }

    public virtual void BuildFields(CsharpStringBuilder strbldr) {}

    public virtual void BuildStart(CsharpStringBuilder strbldr) {}
    public virtual void BuildOnTry(CsharpStringBuilder strbldr) {}

    public virtual void BuildBeforeCall(CsharpStringBuilder strbldr) {}
    public virtual void BuildAfterCall(CsharpStringBuilder strbldr) {}

    public virtual void BuildOnHandleException(CsharpStringBuilder strbldr) {}
    public virtual void BuildOnExceptionHandled(CsharpStringBuilder strbldr) {}
    
    public virtual void BuildEnd(CsharpStringBuilder strbldr) {}
    
    public abstract int Priority { get; }
}