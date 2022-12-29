namespace Reed.Generators;

public class TimeoutPolicySourceWriter : IPolicySourceWriter
{
    public void WriteBefore(CsharpStringBuilder strbldr)
    {
        strbldr.AppendLine("// timeout policy prefix");
        strbldr.AppendLine("Console.WriteLine(_resiliencyPolicy.OptimisticTimeout);");
    }

    public void WriteAfter(CsharpStringBuilder strbldr)
    {
        strbldr.AppendLine("// timeout policy suffix");
    }

    public void WriteFields(CsharpStringBuilder strbldr)
    {
        
    }
}