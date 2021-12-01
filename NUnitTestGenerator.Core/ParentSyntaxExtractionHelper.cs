using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NUnitTestGenerator.Core;

public static class ParentSyntaxExtractionHelper
{
    public static void GetParentSyntax<T>(SyntaxNode syntaxNode, out T? parentSyntax) where T : SyntaxNode
    {
        while (true)
        {
            SyntaxNode? parent = syntaxNode.Parent;

            if (parent == null)
            {
                parentSyntax = null;
                return;
            }

            if (parent.GetType() == typeof(T))
            {
                parentSyntax = parent as T;
                return;
            }

            syntaxNode = parent;
        }
    }

    public static IList<ClassDeclarationSyntax> GetOuterClassesSyntax(SyntaxNode syntaxNode)
    {
        var outerClasses = new List<ClassDeclarationSyntax>();
        SyntaxNode? parent = syntaxNode.Parent;
        while (parent.IsKind(SyntaxKind.ClassDeclaration))
        {
            var outerClass = (parent as ClassDeclarationSyntax)!;
            outerClasses.Add(outerClass);
            parent = parent.Parent;
        }

        outerClasses.Reverse();
        return outerClasses;
    }
}
