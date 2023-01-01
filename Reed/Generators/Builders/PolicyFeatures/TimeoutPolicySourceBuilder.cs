namespace Reed.Generators;

public class TimeoutPolicySourceBuilder : PolicyFeatureSourceBuilder
{
    public override int Priority { get; }
    
    public TimeoutPolicySourceBuilder(ResilientMethodSourceBuilder methodBuilder) : base(methodBuilder)
    {
    }

    public override void BuildFields(CsharpStringBuilder strbldr)
    {
    }

    public override void BuildBefore(CsharpStringBuilder strbldr)
    {
        strbldr.AppendLine("// timeout policy prefix");
        strbldr.AppendLine("Console.WriteLine(resiliencyPolicy.OptimisticTimeout);");
    }

    public override void BuildAfter(CsharpStringBuilder strbldr)
    {
        strbldr.AppendLine("// timeout policy suffix");
    }
}