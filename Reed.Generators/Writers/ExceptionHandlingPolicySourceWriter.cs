namespace Reed.Generators;

public class ExceptionHandlingPolicySourceWriter : IPolicySourceWriter
{
    public IEnumerable<string> GetPrefixLines()
    {
        yield return "try {";
    }

    public IEnumerable<string> GetSuffixLines()
    {
        yield return "} catch(Exception ex) {";
        //yield return "return Task.CompletedTask;";
        //yield return """Console.WriteLine("Exception handled: " + ex);""";
        yield return "}";
    }

    public IEnumerable<string> GetFieldsLines()
    {
        return Enumerable.Empty<string>();
    }
}