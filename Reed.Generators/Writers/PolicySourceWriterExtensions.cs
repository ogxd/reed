using Microsoft.CodeAnalysis;

namespace Reed.Generators;

public static class PolicySourceWriterExtensions
{
    public static IEnumerable<IPolicySourceWriter> GetWritersForPolicySymbol(this ITypeSymbol policySymbol)
    {
        var interfaces = policySymbol.AllInterfaces;
        if (policySymbol is INamedTypeSymbol namedSymbol)
            interfaces = interfaces.Add(namedSymbol);

        return interfaces.Select(IPolicySourceWriter.GetPolicyWriter).Where(x => x != null)!;
    }
}