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
            _ => null,
        };
    }

    public static void WriteBefore(IReadOnlyList<INamedTypeSymbol> policyInterfacesSymbols, CsharpStringBuilder strbldr)
    {
        foreach (INamedTypeSymbol policyInterfaceSymbol in policyInterfacesSymbols)
        {
            var writer = GetPolicyWriter(policyInterfaceSymbol);
            writer?.WriteBefore(strbldr);
        }
    }
    
    public static void WriteAfter(IReadOnlyList<INamedTypeSymbol> policyInterfacesSymbols, CsharpStringBuilder strbldr)
    {
        foreach (INamedTypeSymbol policyInterfaceSymbol in policyInterfacesSymbols)
        {
            var writer = GetPolicyWriter(policyInterfaceSymbol);
            writer?.WriteAfter(strbldr);
        }
    }
}