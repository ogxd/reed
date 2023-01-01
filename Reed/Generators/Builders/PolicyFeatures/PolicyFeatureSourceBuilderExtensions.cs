using Microsoft.CodeAnalysis;

namespace Reed.Generators;

public static class PolicyFeatureSourceBuilderExtensions
{
    public static IEnumerable<Func<ResilientMethodSourceBuilder, PolicyFeatureSourceBuilder>> GetWritersForPolicySymbol(this ITypeSymbol policySymbol)
    {
        var interfaces = policySymbol.AllInterfaces;
        if (policySymbol is INamedTypeSymbol namedSymbol)
            interfaces = interfaces.Add(namedSymbol);

        return interfaces.Select(GetPolicyWriter).Where(x => x != null)!;
    }
    
    public static Func<ResilientMethodSourceBuilder, PolicyFeatureSourceBuilder>? GetPolicyWriter(INamedTypeSymbol policyInterfaceSymbol)
    {
        return policyInterfaceSymbol.Name switch
        {
            nameof(IOptimisticTimeoutPolicy) => methodBuilder => new TimeoutPolicySourceBuilder(methodBuilder),
            nameof(IExceptionHandlingPolicy) => methodBuilder => new ExceptionHandlingPolicySourceBuilder(methodBuilder),
            nameof(ICircuitBreakerPolicy) => methodBuilder => new CircuitBreakerPolicySourceBuilder(methodBuilder),
            _ => null,
        };
    }
}