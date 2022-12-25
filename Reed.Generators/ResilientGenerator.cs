using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reed.Generators;

[Generator]
public class ResilientGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a factory that can create our custom syntax receiver
        context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // the generator infrastructure will create a receiver and populate it
        // we can retrieve the populated instance via the context
        MySyntaxReceiver syntaxReceiver = (MySyntaxReceiver)context.SyntaxReceiver;

        // get the recorded user class
        ClassDeclarationSyntax userClass = syntaxReceiver.ClassToAugment;
        if (userClass is null)
        {
            // if we didn't find the user class, there is nothing to do
            return;
        }

        StringBuilder strbldr = new();
        
        string fullName;
        
        if (TryGetParentSyntax(userClass, out NamespaceDeclarationSyntax namespaceDeclarationSyntax))
        {
            strbldr.AppendLine($"namespace {namespaceDeclarationSyntax.Name};");
            fullName = $"{namespaceDeclarationSyntax.Name}.{userClass.Identifier}";
        }
        else
        {
            fullName = userClass.Identifier.ToString();
        }

        strbldr.Append($@"
public partial class {userClass.Identifier}
{{
    public void Coucou()
    {{
        // generated code
    }}
}}");
        
        SourceText sourceText = SourceText.From(strbldr.ToString(), Encoding.UTF8);
        
        context.AddSource($"{fullName}.Generated.cs", sourceText);
    }
    
    public static bool TryGetParentSyntax<T>(SyntaxNode syntaxNode, out T result) 
        where T : SyntaxNode
    {
        result = null;

        if (syntaxNode == null)
        {
            return false;
        }

        try
        {
            syntaxNode = syntaxNode.Parent;

            if (syntaxNode == null)
            {
                return false;
            }

            if (syntaxNode.GetType() == typeof (T))
            {
                result = syntaxNode as T;
                return true;
            }

            return TryGetParentSyntax<T>(syntaxNode, out result);
        }
        catch
        {
            return false;
        }
    }
    
    class MySyntaxReceiver : ISyntaxReceiver
    {
        public ClassDeclarationSyntax ClassToAugment { get; private set; }
        
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0)
            {
                if (cds.AttributeLists
                    .SelectMany(e => e.Attributes)
                    .Any(e => e.Name.NormalizeWhitespace().ToFullString() == "Resilient"))
                {
                    ClassToAugment = cds;
                }
            }
        }
    }
}