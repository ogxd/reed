using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reed.Generators;

[Generator]
public class DebugGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {

    }

    public void Execute(GeneratorExecutionContext context)
    {
        StringBuilder strbldr = new();
        
        strbldr.AppendLine("// DEBUG");
        foreach (var text in RscgDebug.Text)
        {
            strbldr.AppendLine("// " + text);
        }

        // Write generated code
        SourceText sourceText = SourceText.From(strbldr.ToString(), Encoding.UTF8);
        context.AddSource($"Debug.Reed.cs", sourceText);
    }
}

public static class RscgDebug
{
    private static List<string> _text = new();
    public static void WriteLine(string text)
    {
        _text.Add(text);
    }

    public static IReadOnlyList<string> Text => _text;
}