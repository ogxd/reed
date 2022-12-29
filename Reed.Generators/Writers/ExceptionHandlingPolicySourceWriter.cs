namespace Reed.Generators;

public class ExceptionHandlingPolicySourceWriter : IPolicySourceWriter
{
    public void WriteBefore(CsharpStringBuilder strbldr)
    {
        strbldr.AppendLine("try");
        strbldr.OpenBracket();
    }

    public void WriteAfter(CsharpStringBuilder strbldr)
    {
        strbldr.CloseBracket();
        strbldr.AppendLine("catch (Exception ex)");
        strbldr.OpenBracket();
        strbldr.NewLine();
        strbldr.CloseBracket();
    }

    public void WriteFields(CsharpStringBuilder strbldr)
    {
        
    }
}