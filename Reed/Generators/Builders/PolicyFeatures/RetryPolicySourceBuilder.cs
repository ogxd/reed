namespace Reed.Generators;

public class RetryPolicySourceBuilder : PolicyFeatureSourceBuilder
{
    public override int Priority => 2;
    
    public RetryPolicySourceBuilder(ResilientMethodSourceBuilder methodBuilder) : base(methodBuilder)
    {
    }
    
    public override void BuildStart(CsharpStringBuilder strbldr)
    {
        strbldr.AppendLine("int attempt = 0;");
        strbldr.AppendLine("retry:;");
    }

    public override void BuildOnExceptionHandled(CsharpStringBuilder strbldr)
    {
        strbldr.AppendLine("if (attempt < resiliencyPolicy.RetryAttempts)");
        strbldr.OpenBracket();
        strbldr.AppendLine("attempt++;");
        strbldr.AppendLine("goto retry;");
        strbldr.CloseBracket();
    }
}