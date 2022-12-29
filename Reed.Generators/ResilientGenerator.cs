using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reed.Generators;

[Generator]
public class ResilientGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a factory that can create our custom syntax receiver
        context.RegisterForSyntaxNotifications(() => new ResilientAttributeReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // the generator infrastructure will create a receiver and populate it
        // we can retrieve the populated instance via the context
        ResilientAttributeReceiver? syntaxReceiver = context.SyntaxReceiver as ResilientAttributeReceiver;
        
        if (syntaxReceiver is null)
        {
            return;
        }
        
        foreach (var resilientMethod in syntaxReceiver.ResilientMethods)
        {
            WriteResilientImplementation(resilientMethod, context);
        }
    }

    private static void WriteResilientImplementation(ResilientMethodSyntax resilientMethodSyntax, GeneratorExecutionContext context)
    {
        // get the recorded user class
        MethodDeclarationSyntax userMethod = resilientMethodSyntax.MethodDeclarationSyntax;

        if (userMethod.Parent is not ClassDeclarationSyntax userClass)
        {
            // if we didn't find the user class, there is nothing to do
            return;
        }

        // Retreive generic type
        INamedTypeSymbol? attributeTypeSymbol = context.Compilation.GetSemanticModel(resilientMethodSyntax.AttributeSyntax.SyntaxTree).GetTypeInfo(resilientMethodSyntax.AttributeSyntax).ConvertedType as INamedTypeSymbol;
        ITypeSymbol? policyTypeSymbol = attributeTypeSymbol?.TypeArguments.FirstOrDefault();

        if (policyTypeSymbol == null)
        {
            return;
        }

        RscgDebug.WriteLine($"Write to prout.Reed.cs");

        CsharpStringBuilder strbldr = new();
        strbldr.AppendLine("using System;");
        strbldr.AppendLine("using BenchmarkDotNet.Attributes;");
        strbldr.AppendLine("using Microsoft.Extensions.DependencyInjection;"); // ActivatorUtilitiesConstructor's namespace
        strbldr.AppendLine($"using {policyTypeSymbol.ContainingNamespace};"); // Policy namespace
        strbldr.NewLine();

        string @namespace = TypeDeclarationSyntaxExtensions.GetNamespace(userClass);
        string fullname = userClass.Identifier.ToString();

        if (!string.IsNullOrEmpty(@namespace))
        {
            strbldr.AppendLine($"namespace {@namespace};");
            fullname = @namespace + '.' + fullname;
        }

        strbldr.NewLine();
        strbldr.AppendLine($"public partial class {userClass.Identifier}");
        strbldr.OpenBracket();
        strbldr.AppendLine($"private {policyTypeSymbol.Name} _resiliencyPolicy;");
        strbldr.NewLine();
        strbldr.AppendLine($"[ActivatorUtilitiesConstructor]");
        strbldr.AppendLine($"public {userClass.Identifier}({policyTypeSymbol.Name} resiliencyPolicy)");
        strbldr.OpenBracket();
        strbldr.AppendLine("_resiliencyPolicy = resiliencyPolicy;");
        strbldr.CloseBracket();
        strbldr.NewLine();
        strbldr.AppendLine("[Benchmark]");
        strbldr.AppendLine($"public async Task {userMethod.Identifier}_Resilient({string.Join(", ", userMethod.ParameterList.Parameters.Select(x => $"{x.Type} {x.Identifier}"))})");
        strbldr.OpenBracket();

        IPolicySourceWriter.WriteBefore(policyTypeSymbol.AllInterfaces, strbldr);

        //strbldr.AppendLine(userMethod.Body.ToString());
        strbldr.AppendLine($"await {userMethod.Identifier}({string.Join(", ", userMethod.ParameterList.Parameters.Select(x => $"{x.Identifier}"))});");

        IPolicySourceWriter.WriteAfter(policyTypeSymbol.AllInterfaces, strbldr);

        strbldr.CloseBracket();
        strbldr.CloseBracket();

        // strbldr.AppendLine("// DEBUG");
        // foreach (var text in RscgDebug.Text)
        // {
        //     strbldr.AppendLine("// " + text);
        // }

        // Write generated code
        SourceText sourceText = SourceText.From(strbldr.ToString(), Encoding.UTF8);
        context.AddSource($"{fullname}.Reed.cs", sourceText);
    }

    private class ResilientAttributeReceiver : ISyntaxReceiver
    {
        private List<ResilientMethodSyntax> _resilientMethods = new();

        public IReadOnlyList<ResilientMethodSyntax> ResilientMethods => _resilientMethods;

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is MethodDeclarationSyntax mds && mds.AttributeLists.Count > 0)
            {
                var attr = mds.AttributeLists
                    .SelectMany(e => e.Attributes)
                    .FirstOrDefault(e => e.Name.NormalizeWhitespace().ToFullString().StartsWith("Resilient"));
                
                if (attr != null)
                {
                    _resilientMethods.Add(new (mds, attr));
                }
            }
        }
    }

    public record ResilientMethodSyntax(MethodDeclarationSyntax MethodDeclarationSyntax, AttributeSyntax AttributeSyntax);

    public static class RscgDebug
    {
        private static List<string> _text = new();
        public static void WriteLine(string text)
        {
            _text.Add(text);
        }

        public static IReadOnlyList<string> Text => _text;
    }
}