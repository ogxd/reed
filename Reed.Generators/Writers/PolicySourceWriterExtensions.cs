using Microsoft.CodeAnalysis;

namespace Reed.Generators;

public static class PolicySourceWriterExtensions
{
    public static void WriteBefore(this CsharpStringBuilder strbldr, ITypeSymbol policySymbol)
    {
        var interfaces = policySymbol.AllInterfaces;
        if (policySymbol is INamedTypeSymbol namedSymbol)
            interfaces = interfaces.Add(namedSymbol);
        
        foreach (INamedTypeSymbol policyInterfaceSymbol in interfaces)
        {
            var writer = IPolicySourceWriter.GetPolicyWriter(policyInterfaceSymbol);
            writer?.WriteBefore(strbldr);
        }
    }
    
    public static void WriteAfter(this CsharpStringBuilder strbldr, ITypeSymbol policySymbol)
    {
        var interfaces = policySymbol.AllInterfaces;
        if (policySymbol is INamedTypeSymbol namedSymbol)
            interfaces = interfaces.Add(namedSymbol);
        
        foreach (INamedTypeSymbol policyInterfaceSymbol in interfaces)
        {
            var writer = IPolicySourceWriter.GetPolicyWriter(policyInterfaceSymbol);
            writer?.WriteAfter(strbldr);
        }
    }
    
    public static void WriteFields(this CsharpStringBuilder strbldr, ITypeSymbol policySymbol)
    {
        var interfaces = policySymbol.AllInterfaces;
        if (policySymbol is INamedTypeSymbol namedSymbol)
            interfaces = interfaces.Add(namedSymbol);
        
        foreach (INamedTypeSymbol policyInterfaceSymbol in interfaces)
        {
            var writer = IPolicySourceWriter.GetPolicyWriter(policyInterfaceSymbol);
            writer?.WriteFields(strbldr);
        }
    }
}