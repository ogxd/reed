namespace Reed.Generators;

public class ExceptionHandlingPolicySourceBuilder : PolicyFeatureSourceBuilder
{
    public override int Priority => 0;
    
    public ExceptionHandlingPolicySourceBuilder(ResilientMethodSourceBuilder methodBuilder) : base(methodBuilder)
    {
    }
    
    public override void BuildOnTry(CsharpStringBuilder strbldr)
    {
        strbldr.AppendLine("try");
        strbldr.OpenBracket();
    }

    public override void BuildOnHandleException(CsharpStringBuilder strbldr)
    {
        strbldr.CloseBracket();
        strbldr.AppendLine("catch (Exception exception) when (resiliencyPolicy.IsExceptionHandled(exception))");
        strbldr.OpenBracket();
    }
    
    public override void BuildEnd(CsharpStringBuilder strbldr)
    {
        strbldr.CloseBracket();
    }
}