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

        // get the recorded user class
        MethodDeclarationSyntax? userMethod = syntaxReceiver?.MethodToAugment;
        if (userMethod is null)
        {
            // if we didn't find the user class, there is nothing to do
            return;
        }
        
        ClassDeclarationSyntax userClass = userMethod.Parent as ClassDeclarationSyntax;
        if (userClass is null)
        {
            // if we didn't find the user class, there is nothing to do
            return;
        }
        
        StringBuilder strbldr = new();
        strbldr.AppendLine("using System;");
        strbldr.AppendLine("using Microsoft.Extensions.DependencyInjection;");

        string @namespace = TypeDeclarationSyntaxExtensions.GetNamespace(userClass);
        string fullname = userClass.Identifier.ToString();
        
        if (!string.IsNullOrEmpty(@namespace))
        {
            strbldr.AppendLine($"namespace {@namespace};");
            fullname = @namespace + '.' + fullname;
        }

        INamedTypeSymbol type = context.Compilation.GetSemanticModel(syntaxReceiver.AttributeSyntax.SyntaxTree).GetTypeInfo(syntaxReceiver.AttributeSyntax).ConvertedType as INamedTypeSymbol;
        //INamedTypeSymbol type = context.Compilation.GetTypeByMetadataName(syntaxReceiver.AttributeSyntax.Name.ToString());
        //RscgDebug.WriteLine(">>> " + sem?.TypeArguments.FirstOrDefault()?.Name);
        ITypeSymbol? policyTypeSymbol = type?.TypeArguments.FirstOrDefault();




        strbldr.AppendLine($$"""
        public partial class {{userClass.Identifier}}
        {
            private {{policyTypeSymbol?.Name}} _resiliencyPolicy;

            [ActivatorUtilitiesConstructor]
            public {{userClass.Identifier}}({{policyTypeSymbol?.Name}} resiliencyPolicy) {
                _resiliencyPolicy = resiliencyPolicy;
            }
        
            public void {{userMethod.Identifier}}_Resilient({{string.Join(", ", userMethod.ParameterList.Parameters.Select(x => $"{x.Type} {x.Identifier}"))}})
            {
        """);
        
        foreach (var prefix in IPolicySourceWriter.GetPrefixes(policyTypeSymbol.AllInterfaces))
        {
            strbldr.AppendLine(prefix);
        }

        strbldr.AppendLine($$"""
                // We have two options: {@namespace}d
                // - Copy the whole method body, so that we don't create another async state machine. Better performance, but shitty debugging experience.
                // userMethod.Body.ToString()
                // - Just call the original method.
                {{userMethod.Identifier}}({{string.Join(", ", userMethod.ParameterList.Parameters.Select(x => $"{x.Identifier}"))}});
        """);
        
        foreach (var suffix in IPolicySourceWriter.GetSuffixes(policyTypeSymbol.AllInterfaces))
        {
            strbldr.AppendLine(suffix);
        }
        
        strbldr.AppendLine($$"""
            }
        }
        """);

        strbldr.AppendLine("// DEBUG");
        foreach (var text in RscgDebug.Text)
        {
            strbldr.AppendLine("// " + text);
        }
        
        // var attribute = context.Compilation.Assembly.GetAttributes().FirstOrDefault(x => x.AttributeClass.ToString() == "Helloworld.TestAttribute1");
        
        SourceText sourceText = SourceText.From(strbldr.ToString(), Encoding.UTF8);
        
        context.AddSource($"{fullname}.Reed.cs", sourceText);
    }
     
    private class ResilientAttributeReceiver : ISyntaxReceiver
    {
        public MethodDeclarationSyntax MethodToAugment { get; private set; }
        public AttributeSyntax AttributeSyntax { get; private set; }
        
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is MethodDeclarationSyntax mds && mds.AttributeLists.Count > 0)
            {
                var attr = mds.AttributeLists
                    .SelectMany(e => e.Attributes)
                    .FirstOrDefault(e => e.Name.NormalizeWhitespace().ToFullString().StartsWith("Resilient"));
                
                if (attr != null)
                {
                    MethodToAugment = mds;
                    AttributeSyntax = attr;
                }
            }
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
}