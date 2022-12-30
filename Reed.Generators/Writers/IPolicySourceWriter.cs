using Microsoft.CodeAnalysis;

namespace Reed.Generators;

public interface IPolicySourceWriter
{
    void WriteBefore(CsharpStringBuilder strbldr);
    void WriteAfter(CsharpStringBuilder strbldr);
    void WriteFields(CsharpStringBuilder strbldr);

    public static IPolicySourceWriter? GetPolicyWriter(INamedTypeSymbol policyInterfaceSymbol)
    {
        return policyInterfaceSymbol.Name switch
        {
            nameof(IOptimisticTimeoutPolicy) => new TimeoutPolicySourceWriter(),
            nameof(IExceptionHandlingPolicy) => new ExceptionHandlingPolicySourceWriter(),
            nameof(ICircuitBreakerPolicy) => new CircuitBreakerPolicySourceWriter(),
            _ => null,
        };
    }
}