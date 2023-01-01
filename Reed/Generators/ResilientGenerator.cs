using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reed.Generators;

public interface ICaca
{
    
}

[Generator]
public class ResilientGenerator : ISourceGenerator
{
    private INamedTypeSymbol? _taskSymbol;
    private INamedTypeSymbol? _taskTSymbol;

    // Lazy initialized
    private INamedTypeSymbol? GetTaskSymbol(Compilation compilation) => _taskSymbol ??= compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
    private INamedTypeSymbol? GetTaskTSymbol(Compilation compilation) => _taskTSymbol ??= compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
    
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
        
        foreach (var pair in syntaxReceiver.ResilientMethods)
        {
            try
            {
                ResilientClassSourceBuilder classBuilder = new();

                foreach (ResilientMethodSyntax resilientMethodSyntax in pair.Value)
                {
                    classBuilder.AddResilientMethod(x => WriteResilientImplementation(x, pair.Key, resilientMethodSyntax, context));
                }

                CsharpStringBuilder strbldr = new();
            
                classBuilder.Build(strbldr);

                // Write generated code
                SourceText sourceText = SourceText.From(strbldr.ToString(), Encoding.UTF8);
                context.AddSource($"{classBuilder.FullName}.Reed.cs", sourceText);
            }
            catch (Exception ex)
            {
                context.LogError("Could not write resilient class: " + ex.ToString().Replace(Environment.NewLine, " / ")); // TODO: Proper error code
            }
        }
    }

    private bool WriteResilientImplementation(ResilientMethodSourceBuilder methodSourceBuilder, ClassDeclarationSyntax userClass, ResilientMethodSyntax resilientMethodSyntax, GeneratorExecutionContext context)
    {
        try
        {
            if (!userClass.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            {
                context.LogError($"Class '{userClass.Identifier}' must be partial"); // TODO: Custom error code
                return false;
            }
            
            // get the recorded user class
            MethodDeclarationSyntax userMethod = resilientMethodSyntax.MethodDeclarationSyntax;

            string? methodName = $"{userMethod.Identifier}_Resilient";
            
            // TODO: Use sematic model in more places or make some extension method
            SemanticModel methodSemanticModel = context.Compilation.GetSemanticModel(resilientMethodSyntax.MethodDeclarationSyntax.SyntaxTree);
            IMethodSymbol? methodSymbol = methodSemanticModel.GetDeclaredSymbol(resilientMethodSyntax.MethodDeclarationSyntax);
            
            foreach (AttributeData attributeData in methodSymbol.GetAttributes())
            {
                if (attributeData.ConstructorArguments.Length == 0)
                    continue;
                
                TypedConstant firstArgument = attributeData.ConstructorArguments[0];
                methodName = firstArgument.Value?.ToString();
                
                if (string.IsNullOrEmpty(methodName))
                {
                    context.Log0011(resilientMethodSyntax.AttributeSyntax.GetLocation());
                    return false;
                }
            }
            
            if (methodName == userMethod.Identifier.ToString())
            {
                context.Log0010(userMethod.GetLocation(), methodName);
                return false;
            }
            
            methodSourceBuilder.WithName(userMethod.Identifier.ToString());
            methodSourceBuilder.WithCustomName(methodName);
            
            // Retreive generic type
            SemanticModel semanticModel = context.Compilation.GetSemanticModel(resilientMethodSyntax.AttributeSyntax.SyntaxTree);
            INamedTypeSymbol? attributeTypeSymbol = semanticModel.GetTypeInfo(resilientMethodSyntax.AttributeSyntax).ConvertedType as INamedTypeSymbol;
            ITypeSymbol? policyTypeSymbol = attributeTypeSymbol?.TypeArguments.FirstOrDefault();

            if (policyTypeSymbol == null)
            {
                context.LogError("Policy symbol not found");
                return false;
            }
            
            bool isNonGenericTask = SymbolEqualityComparer.Default.Equals(methodSymbol.ReturnType, GetTaskSymbol(context.Compilation));
            bool isAwaitable =  isNonGenericTask || SymbolEqualityComparer.Default.Equals(methodSymbol.ReturnType, GetTaskTSymbol(context.Compilation));
            bool noReturn = methodSymbol.ReturnsVoid || isNonGenericTask;

            if (isAwaitable)
            {
                methodSourceBuilder.WithAsync();
            }

            if (!noReturn)
            {
                methodSourceBuilder.WithReturnType(methodSymbol.ReturnType.Name);
            }

            // TODO: Move out
            methodSourceBuilder.ClassSourceBuilder.WithNamespace(userClass.GetNamespace());
            methodSourceBuilder.ClassSourceBuilder.WithName(userClass.Identifier.ToString());
            
            methodSourceBuilder.WithResiliencyPolicy(policyTypeSymbol.ContainingNamespace.ToString() /*Handles multi level namespaces?*/, policyTypeSymbol.Name);

            foreach (var factory in policyTypeSymbol.GetWritersForPolicySymbol())
            {
                methodSourceBuilder.AddPolicyFeature(factory);
            }

            methodSourceBuilder.WithArguments(userMethod.ParameterList.Parameters.Select(x => (x.Type.ToString(), x.Identifier.ToString())).ToList());

            return true;
        }
        catch (Exception ex)
        {
            context.LogError("Unhandled exception: " + ex);
            return false;
        }
    }

    private class ResilientAttributeReceiver : ISyntaxReceiver
    {
        private readonly Dictionary<ClassDeclarationSyntax, List<ResilientMethodSyntax>> _resilientMethodsPerClass = new();

        public IReadOnlyDictionary<ClassDeclarationSyntax, List<ResilientMethodSyntax>> ResilientMethods => _resilientMethodsPerClass;

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (ResilientMethodSyntax.TryGetResilientMethod(syntaxNode, out var resilientMethodSyntax))
            {
                if (resilientMethodSyntax.MethodDeclarationSyntax.Parent is ClassDeclarationSyntax userClass)
                {
                    ref List<ResilientMethodSyntax> value = ref CollectionsMarshal.GetValueRefOrAddDefault(_resilientMethodsPerClass, userClass, out bool exists)!;
                    if (!exists)
                    {
                        value = new();
                    }
                    value.Add(resilientMethodSyntax);
                }
            }
        }
    }

    public class ResilientMethodSyntax
    {
        public MethodDeclarationSyntax MethodDeclarationSyntax { get; }
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