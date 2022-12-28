using Microsoft.CodeAnalysis;

namespace Reed.Generators;

public interface IPolicySourceWriter
{
    IEnumerable<string> GetPrefixLines();
    IEnumerable<string> GetSuffixLines();

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

public class TimeoutPolicySourceWriter : IPolicySourceWriter
{
    public IEnumerable<string> GetPrefixLines()
    {
        yield return "// timeout policy prefix";
        yield return "Console.WriteLine(_resiliencyPolicy.OptimisticTimeout);";
    }

    public IEnumerable<string> GetSuffixLines()
    {
        yield return "// timeout policy suffix";
    }
}

public class ExceptionHandlingPolicySourceWriter : IPolicySourceWriter
{
    public IEnumerable<string> GetPrefixLines()
    {
        yield return "try {";
    }

    public IEnumerable<string> GetSuffixLines()
    {
        yield return "} catch(Exception ex) {";
        yield return """Console.WriteLine("Exception handled: " + ex);""";
        yield return "}";
    }
}