using System.Diagnostics;
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
            try
            {
                WriteResilientImplementation(resilientMethod, context);
            }
            catch (Exception ex)
            {
                context.LogError(ex.ToString());
                RscgDebug.WriteLine(ex.ToString());
            }
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
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(resilientMethodSyntax.AttributeSyntax.SyntaxTree);
        INamedTypeSymbol? attributeTypeSymbol = semanticModel.GetTypeInfo(resilientMethodSyntax.AttributeSyntax).ConvertedType as INamedTypeSymbol;
        ITypeSymbol? policyTypeSymbol = attributeTypeSymbol?.TypeArguments.FirstOrDefault();

        if (policyTypeSymbol == null)
        {
            return;
        }
        
        string methodName = $"{userMethod.Identifier}_Resilient";
        
        // TODO: Use sematic model in more places or make some extension methods
        SemanticModel methodSemanticModel = context.Compilation.GetSemanticModel(resilientMethodSyntax.MethodDeclarationSyntax.SyntaxTree);
        IMethodSymbol s = methodSemanticModel.GetDeclaredSymbol(resilientMethodSyntax.MethodDeclarationSyntax);
        foreach (AttributeData attributeData in s.GetAttributes())
        {
            if (attributeData.ConstructorArguments.Length == 0)
                continue;
            TypedConstant firstArgument = attributeData.ConstructorArguments[0];
            methodName = firstArgument.Value?.ToString();
            
            if (string.IsNullOrEmpty(methodName))
            {
                context.Log0011(resilientMethodSyntax.AttributeSyntax.GetLocation());
                return;
            }
        }
        
        if (methodName == userMethod.Identifier.ToString())
        {
            context.Log0010(userMethod.GetLocation(), methodName);
            return;
        }

        CsharpStringBuilder strbldr = new();
        strbldr.AppendLine("using System;");
        strbldr.AppendLine("using Microsoft.Extensions.DependencyInjection;"); // ActivatorUtilitiesConstructor's namespace
        strbldr.AppendLine($"using {policyTypeSymbol.ContainingNamespace};"); // Policy namespace
        strbldr.NewLine();

        string? @namespace = TypeDeclarationSyntaxExtensions.GetNamespace(userClass);
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
        strbldr.WriteFields(policyTypeSymbol);
        strbldr.NewLine();
        
        strbldr.AppendLine($"[ActivatorUtilitiesConstructor]");
        strbldr.AppendLine($"public {userClass.Identifier}({policyTypeSymbol.Name} resiliencyPolicy)");
        strbldr.OpenBracket();
        strbldr.AppendLine("_resiliencyPolicy = resiliencyPolicy;");
        strbldr.CloseBracket();
        strbldr.NewLine();
        
        // TODO: Handle all return types (including async)
        strbldr.AppendLine($"public async Task {methodName}({string.Join(", ", userMethod.ParameterList.Parameters.Select(x => $"{x.Type} {x.Identifier}"))})");
        strbldr.OpenBracket();

        strbldr.WriteBefore(policyTypeSymbol);

        // 2 flavors : Rewrite method OR just call the original method
        //strbldr.AppendLine(userMethod.Body.ToString());
        strbldr.AppendLine($"await {userMethod.Identifier}({string.Join(", ", userMethod.ParameterList.Parameters.Select(x => $"{x.Identifier}"))});");

        strbldr.WriteAfter(policyTypeSymbol);

        strbldr.CloseBracket();
        strbldr.CloseBracket();

        RscgDebug.WriteLine($"Write to {fullname}.Reed.cs");
        
        // Write generated code
        SourceText sourceText = SourceText.From(strbldr.ToString(), Encoding.UTF8);
        context.AddSource($"{fullname}.Reed.cs", sourceText);
    }

    private class ResilientAttributeReceiver : ISyntaxReceiver
    {
        private readonly List<ResilientMethodSyntax> _resilientMethods = new();

        public IReadOnlyList<ResilientMethodSyntax> ResilientMethods => _resilientMethods;

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (ResilientMethodSyntax.TryGetResilientMethod(syntaxNode, out var resilientMethodSyntax))
            {
                _resilientMethods.Add(resilientMethodSyntax);
            }
        }
    }

    public class ResilientMethodSyntax
    {
        public MethodDeclarationSyntax MethodDeclarationSyntax{ get; }
        public AttributeSyntax AttributeSyntax { get; }
        
        private ResilientMethodSyntax(MethodDeclarationSyntax methodDeclarationSyntax, AttributeSyntax attributeSyntax)
        {
            AttributeSyntax = attributeSyntax;
            MethodDeclarationSyntax = methodDeclarationSyntax;
        }

        public static bool TryGetResilientMethod(SyntaxNode syntaxNode, out ResilientMethodSyntax resilientMethodSyntax)
        {
            resilientMethodSyntax = null!;

            if (syntaxNode is not MethodDeclarationSyntax mds)
                return false;

            if (mds.AttributeLists.Count == 0)
                return false;
            
            var resilientAttributeSyntaxNode = mds.AttributeLists
                .SelectMany(e => e.Attributes)
                .FirstOrDefault(e => e.Name.NormalizeWhitespace().ToFullString().StartsWith("Resilient"));
            
            if (resilientAttributeSyntaxNode == null)
                return false;

            resilientMethodSyntax = new ResilientMethodSyntax(mds, resilientAttributeSyntaxNode);

            return true;
        }
    }
}