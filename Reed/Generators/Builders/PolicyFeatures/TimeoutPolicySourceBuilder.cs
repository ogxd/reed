namespace Reed.Generators;

public class TimeoutPolicySourceBuilder : PolicyFeatureSourceBuilder
{
    public override int Priority { get; }
    
    public TimeoutPolicySourceBuilder(ResilientMethodSourceBuilder methodBuilder) : base(methodBuilder)
    {
    }
}