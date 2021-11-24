using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnitTestGenerator.ItemInfo;

namespace NUnitTestGenerator
{
    internal static class SourcesParser
    {
        private static bool HasPublicKeyword(SyntaxToken modifier) => modifier.IsKind(SyntaxKind.PublicKeyword);

        private static bool IsPublicMember(MemberDeclarationSyntax memberDeclaration) =>
            memberDeclaration.Modifiers.Any(HasPublicKeyword);

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
            var typeInformation = new TypeInformation(classDeclaration.Identifier.ValueText, methods, constructors);
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
}
