using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnitTestGenerator.Core.ItemInfo;

namespace NUnitTestGenerator.Core;

internal static class SourcesParser
{
    private static readonly Func<ParameterSyntax, string> ParameterNameSelector =
        parameter => parameter.Identifier.Text;

    private static readonly Func<ParameterSyntax, string> ParameterTypeSelector =
        parameter => parameter.Type is not null
                         ? parameter.Type.ToString()
                         : nameof(Object).ToLower();

    public static SourceFileInformation GetSourceFileInfo(string sourceFileContent)
    {
        CompilationUnitSyntax compilationUnitRoot =
            CSharpSyntaxTree.ParseText(sourceFileContent).GetCompilationUnitRoot();
        IList<TypeInformation> classes = compilationUnitRoot.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Select(GetClassInfo)
            .ToList();
        var sourceFileInformation = new SourceFileInformation(classes);
        return sourceFileInformation;
    }

    private static bool HasPublicKeyword(SyntaxToken modifier)
    {
        return modifier.IsKind(SyntaxKind.PublicKeyword);
    }

    private static bool IsPublicMember(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration.Modifiers.Any(HasPublicKeyword);
    }

    private static TypeInformation GetClassInfo(ClassDeclarationSyntax classDeclaration)
    {
        IList<MethodInformation> methods = classDeclaration.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(IsPublicMember)
            .Select(GetMethodInfo)
            .ToList();
        List<ConstructorInformation> constructors = classDeclaration.DescendantNodes()
            .OfType<ConstructorDeclarationSyntax>()
            .Where(IsPublicMember)
            .Select(GetConstructorInfo)
            .ToList();
        ParentSyntaxExtractionHelper.GetParentSyntax(classDeclaration,
                                                     out NamespaceDeclarationSyntax? namespaceDeclaration);
        IList<ClassDeclarationSyntax> outerClasses =
            ParentSyntaxExtractionHelper.GetOuterClassesSyntax(classDeclaration);
        string classInnerName = string.Join('.', outerClasses.Select(clazz => clazz.Identifier.ValueText)
                                                .Append(classDeclaration.Identifier.ValueText));
        var typeInformation = new TypeInformation(namespaceDeclaration?.Name.ToString(),
                                                  classDeclaration.Identifier.ValueText,
                                                  classInnerName, methods, constructors);
        return typeInformation;
    }

    private static MethodInformation GetMethodInfo(MethodDeclarationSyntax method)
    {
        string methodName = method.Identifier.ValueText;
        var returnType = method.ReturnType.ToString();
        IDictionary<string, string> parametersNameTypeDictionary =
            method.ParameterList.Parameters.ToDictionary(ParameterNameSelector, ParameterTypeSelector);
        var methodInformation = new MethodInformation(methodName, returnType, parametersNameTypeDictionary);
        return methodInformation;
    }

    private static ConstructorInformation GetConstructorInfo(ConstructorDeclarationSyntax constructor)
    {
        string constructorName = constructor.Identifier.ValueText;
        IDictionary<string, string> parametersNameTypeDictionary =
            constructor.ParameterList.Parameters.ToDictionary(ParameterNameSelector, ParameterTypeSelector);
        var constructorInformation = new ConstructorInformation(constructorName, parametersNameTypeDictionary);
        return constructorInformation;
    }
}
