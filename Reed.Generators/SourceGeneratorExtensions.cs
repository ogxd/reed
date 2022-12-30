using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reed.Generators;

public static class SourceGeneratorExtensions
{
    public static IMethodSymbol? GetSymbol(this MethodDeclarationSyntax syntaxNode, GeneratorExecutionContext context) => context.Compilation.GetSemanticModel(syntaxNode.SyntaxTree).GetDeclaredSymbol(syntaxNode);
    public static IPropertySymbol? GetSymbol(this PropertyDeclarationSyntax syntaxNode, GeneratorExecutionContext context) => context.Compilation.GetSemanticModel(syntaxNode.SyntaxTree).GetDeclaredSymbol(syntaxNode);
    public static INamedTypeSymbol? GetSymbol(this ClassDeclarationSyntax syntaxNode, GeneratorExecutionContext context) => context.Compilation.GetSemanticModel(syntaxNode.SyntaxTree).GetDeclaredSymbol(syntaxNode);
    public static ISymbol? GetSymbol(this SyntaxNode syntaxNode, GeneratorExecutionContext context) => context.Compilation.GetSemanticModel(syntaxNode.SyntaxTree).GetDeclaredSymbol(syntaxNode);
    public static SymbolInfo GetSymbolInfo(this SyntaxNode syntaxNode, GeneratorExecutionContext context) => context.Compilation.GetSemanticModel(syntaxNode.SyntaxTree).GetSymbolInfo(syntaxNode);
}