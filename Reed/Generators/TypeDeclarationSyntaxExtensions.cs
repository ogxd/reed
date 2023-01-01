using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reed.Generators;

public static class TypeDeclarationSyntaxExtensions
{   
    public static string GetNamespace(this TypeDeclarationSyntax source)
    {
        ArgumentNullException.ThrowIfNull(source);
        
        var namespaces = new Stack<BaseNamespaceDeclarationSyntax>();
        for (var parent = source.Parent; parent is not null; parent = parent.Parent)
        {
            if (parent is BaseNamespaceDeclarationSyntax @namespace)
            {
                namespaces.Push(@namespace);
            }
        }

        return string.Join('.', namespaces.Select(x => x.Name));
    }
}