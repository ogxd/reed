namespace Reed.Generators;

public class ExceptionHandlingPolicySourceBuilder : PolicyFeatureSourceBuilder
{
    public override int Priority { get; }
    
    public ExceptionHandlingPolicySourceBuilder(ResilientMethodSourceBuilder methodBuilder) : base(methodBuilder)
    {
    }
    
    public override void BuildFields(CsharpStringBuilder strbldr)
    {   
    }
    
    public override void BuildBefore(CsharpStringBuilder strbldr)
    {
        strbldr.AppendLine("try");
        strbldr.OpenBracket();
    }

    public override void BuildAfter(CsharpStringBuilder strbldr)
    {
        strbldr.CloseBracket();
        strbldr.AppendLine("catch (Exception ex)");
        strbldr.OpenBracket();
        strbldr.NewLine();
        strbldr.CloseBracket();
    }
}