namespace Reed.Generators;

public class CircuitBreakerPolicySourceBuilder : PolicyFeatureSourceBuilder
{
    public override int Priority { get; }
    
    public CircuitBreakerPolicySourceBuilder(ResilientMethodSourceBuilder methodBuilder) : base(methodBuilder)
    {
    }

    public override void BuildFields(CsharpStringBuilder strbldr)
    {
        // We want one circuit breaker state per method, hence we make the field use the method ID to be unique per method
        strbldr.AppendLine($"private int _circuitBreakerThreshold{MethodSourceBuilder.Id};"); // TODO: Rename to handle several circuit breaked methods per class
    }
    
    public override void BuildBefore(CsharpStringBuilder strbldr)
    {
        strbldr.AppendLine($"if (_circuitBreakerThreshold{MethodSourceBuilder.Id} > Random.Shared.Next(0, resiliencyPolicy.{nameof(ICircuitBreakerPolicy.CircuitBreakerFailureThreshold)}))");
        strbldr.OpenBracket();
        strbldr.AppendLine("return;");
        strbldr.CloseBracket();
        strbldr.NewLine();
        strbldr.AppendLine("try");
        strbldr.OpenBracket();
    }

    public override void BuildAfter(CsharpStringBuilder strbldr)
    {
        strbldr.AppendLine($"ReedExtensions.DecrementClamped(ref _circuitBreakerThreshold{MethodSourceBuilder.Id}, 0);");
        strbldr.CloseBracket();
        strbldr.AppendLine("catch (Exception exception) when (resiliencyPolicy.IsExceptionHandled(exception))");
        strbldr.OpenBracket();
        strbldr.AppendLine($"ReedExtensions.IncrementClamped(ref _circuitBreakerThreshold{MethodSourceBuilder.Id}, resiliencyPolicy.{nameof(ICircuitBreakerPolicy.CircuitBreakerFailureThreshold)});");
        strbldr.CloseBracket();
    }
}