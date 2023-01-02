namespace Reed.Generators;

public class CircuitBreakerPolicySourceBuilder : PolicyFeatureSourceBuilder
{
    public override int Priority => 1;
    
    public CircuitBreakerPolicySourceBuilder(ResilientMethodSourceBuilder methodBuilder) : base(methodBuilder)
    {
    }

    public override void BuildFields(CsharpStringBuilder strbldr)
    {
        // We want one circuit breaker state per method, hence we make the field use the method ID to be unique per method
        strbldr.AppendLine($"private int _circuitBreakerThreshold{MethodSourceBuilder.Id};"); // TODO: Rename to handle several circuit breaked methods per class
    }
    
    public override void BuildStart(CsharpStringBuilder strbldr)
    {
        strbldr.AppendLine($"if (_circuitBreakerThreshold{MethodSourceBuilder.Id} > Random.Shared.Next(0, resiliencyPolicy.{nameof(ICircuitBreakerPolicy.CircuitBreakerFailureThreshold)}))");
        strbldr.OpenBracket();
        strbldr.AppendLine("return;");
        strbldr.CloseBracket();
        strbldr.NewLine();
    }

    public override void BuildAfterCall(CsharpStringBuilder strbldr)
    {
        strbldr.AppendLine($"ReedExtensions.DecrementClamped(ref _circuitBreakerThreshold{MethodSourceBuilder.Id}, 0);");
    }
    
    public override void BuildOnExceptionHandled(CsharpStringBuilder strbldr)
    {
        strbldr.AppendLine($"ReedExtensions.IncrementClamped(ref _circuitBreakerThreshold{MethodSourceBuilder.Id}, resiliencyPolicy.{nameof(ICircuitBreakerPolicy.CircuitBreakerFailureThreshold)});");
    }
}