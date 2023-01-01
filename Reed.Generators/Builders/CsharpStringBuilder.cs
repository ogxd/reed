using System.Text;

namespace Reed.Generators;

public class CsharpStringBuilder
{
    private StringBuilder _strbldr = new();
    private int _identation = 0;

    public CsharpStringBuilder AppendLine(string line)
    {
        _strbldr.Append(new string(' ', _identation * 4));
        _strbldr.AppendLine(line);
        return this;
    }
    
    public CsharpStringBuilder OpenBracket()
    {
        AppendLine("{");
        _identation++;
        return this;
    }
    
    public CsharpStringBuilder CloseBracket()
    {
        _identation--;
        AppendLine("}");
        return this;
    }
    
    public CsharpStringBuilder NewLine()
    {
        _strbldr.Append(Environment.NewLine);
        return this;
    }

    public override string ToString()
    {
        return _strbldr.ToString();
    }
}