namespace Reed.Generators;

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

    public IEnumerable<string> GetFieldsLines()
    {
        return Enumerable.Empty<string>();
    }
}