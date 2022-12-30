namespace Reed.Generators;

public class CircuitBreakerPolicySourceWriter : IPolicySourceWriter
{
    public void WriteBefore(CsharpStringBuilder strbldr)
    {
        strbldr.AppendLine($"if (_circuitBreakerThreshold > Random.Shared.Next(0, _resiliencyPolicy.{nameof(ICircuitBreakerPolicy.CircuitBreakerFailureThreshold)}))");
        strbldr.OpenBracket();
        strbldr.AppendLine("return;");
        strbldr.CloseBracket();
        strbldr.NewLine();
        strbldr.AppendLine("try");
        strbldr.OpenBracket();
    }

    public void WriteAfter(CsharpStringBuilder strbldr)
    {
        strbldr.AppendLine("ReedExtensions.DecrementClamped(ref _circuitBreakerThreshold, 0);");
        strbldr.CloseBracket();
        strbldr.AppendLine("catch (Exception exception) when (_resiliencyPolicy.IsExceptionHandled(exception))");
        strbldr.OpenBracket();
        strbldr.AppendLine($"ReedExtensions.IncrementClamped(ref _circuitBreakerThreshold, _resiliencyPolicy.{nameof(ICircuitBreakerPolicy.CircuitBreakerFailureThreshold)});");
        strbldr.CloseBracket();
    }

    public void WriteFields(CsharpStringBuilder strbldr)
    {
        strbldr.AppendLine("private int _circuitBreakerThreshold;"); // TODO: Rename to handle several circuit breaked methods per class
    }
}