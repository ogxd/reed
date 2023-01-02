using Microsoft.CodeAnalysis;

namespace Reed.Generators;

public static class PolicyFeatureSourceBuilderExtensions
{
    private static IEnumerable<INamedTypeSymbol> GetInterfacesRecursive(ITypeSymbol symbol)
    {
        if (symbol is INamedTypeSymbol namedSymbol)
            yield return namedSymbol;

        foreach (var @interface in symbol.AllInterfaces)
        {
            foreach (var @namedInterface in GetInterfacesRecursive(@interface))
            {
                yield return namedInterface;
            }
        }
    }

    public static IEnumerable<Func<ResilientMethodSourceBuilder, PolicyFeatureSourceBuilder>> GetWritersForPolicySymbol(this ITypeSymbol policySymbol)
    {
        return GetInterfacesRecursive(policySymbol)
            .Distinct(SymbolEqualityComparer.Default)
            .Select(GetPolicyWriter!)
            .Where(x => x != null)!;
    }
    
    public static Func<ResilientMethodSourceBuilder, PolicyFeatureSourceBuilder>? GetPolicyWriter(ISymbol policyInterfaceSymbol)
    {
        return policyInterfaceSymbol.Name switch
        {
            nameof(IOptimisticTimeoutPolicy) => methodBuilder => new TimeoutPolicySourceBuilder(methodBuilder),
            nameof(IExceptionHandlingPolicy) => methodBuilder => new ExceptionHandlingPolicySourceBuilder(methodBuilder),
            nameof(ICircuitBreakerPolicy) => methodBuilder => new CircuitBreakerPolicySourceBuilder(methodBuilder),
            nameof(IRetryPolicy) => methodBuilder => new RetryPolicySourceBuilder(methodBuilder),
            _ => null,
        };
    }
}