using Microsoft.CodeAnalysis;

namespace Reed.Generators;

public interface IPolicySourceWriter
{
    IEnumerable<string> GetPrefixLines();
    IEnumerable<string> GetSuffixLines();
    IEnumerable<string> GetFieldsLines();

    public static IPolicySourceWriter? GetPolicyWriter(INamedTypeSymbol policyInterfaceSymbol)
    {
        return policyInterfaceSymbol.Name switch
        {
            nameof(IOptimisticTimeoutPolicy) => new TimeoutPolicySourceWriter(),
            nameof(IExceptionHandlingPolicy) => new ExceptionHandlingPolicySourceWriter(),
            _ => null,
        };
    }

    public static IEnumerable<string> GetPrefixes(IReadOnlyList<INamedTypeSymbol> policyInterfacesSymbols)
    {
        foreach (INamedTypeSymbol policyInterfaceSymbol in policyInterfacesSymbols)
        {
            var writer = GetPolicyWriter(policyInterfaceSymbol);
            if (writer == null)
            {
                continue;
            }
            foreach (var line in writer.GetPrefixLines())
            {
                yield return line;
            }
        }
    }
    
    public static IEnumerable<string> GetSuffixes(IReadOnlyList<INamedTypeSymbol> policyInterfacesSymbols)
    {
        foreach (INamedTypeSymbol policyInterfaceSymbol in policyInterfacesSymbols)
        {
            var writer = GetPolicyWriter(policyInterfaceSymbol);
            if (writer == null)
            {
                continue;
            }
            foreach (var line in writer.GetSuffixLines())
            {
                yield return line;
            }
        }
    }
}