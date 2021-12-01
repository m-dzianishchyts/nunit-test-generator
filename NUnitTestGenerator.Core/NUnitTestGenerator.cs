using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using NUnitTestGenerator.Core.ItemInfo;

namespace NUnitTestGenerator.Core;

public static class NUnitTestGenerator
{
    private const string TestClassPostfix = "Test";

    private static readonly SyntaxToken PublicModifier = SyntaxFactory.Token(SyntaxKind.PublicKeyword);
    private static readonly SyntaxToken PrivateModifier = SyntaxFactory.Token(SyntaxKind.PrivateKeyword);

    private static readonly TypeSyntax VoidReturnType =
        SyntaxFactory.ParseTypeName(ReformatToStartWithLower(typeof(void).Name));

    private static readonly AttributeSyntax SetupAttribute =
        SyntaxFactory.Attribute(SyntaxFactory.ParseName(nameof(SetUpAttribute).Replace(nameof(Attribute), null)));

    private static readonly AttributeSyntax MethodAttribute =
        SyntaxFactory.Attribute(SyntaxFactory.ParseName(nameof(TestAttribute).Replace(nameof(Attribute), null)));

    private static readonly AttributeSyntax ClassAttribute =
        SyntaxFactory.Attribute(
            SyntaxFactory.ParseName(nameof(TestFixtureAttribute).Replace(nameof(Attribute), null)));

    public static IDictionary<string, string> GenerateTests(string sourceFileContent)
    {
        SourceFileInformation sourceFileInformation = SourcesParser.GetSourceFileInfo(sourceFileContent);

        IDictionary<string, string> testClassNameContentDictionary =
            sourceFileInformation.Types.ToDictionary(GenerateTestClassName, GenerateTestClassSources);
        return testClassNameContentDictionary;
    }

    private static string GenerateTestClassSources(TypeInformation typeInformation)
    {
        ClassDeclarationSyntax classDeclaration = GenerateClass(typeInformation);

        IList<UsingDirectiveSyntax> usings = new List<UsingDirectiveSyntax>
        {
            // SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(nameof(System))),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(Assert).Namespace!)),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(nameof(Moq))),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(IList<>).Namespace!))
        };

        if (typeInformation.NamespaceName is not null)
        {
            usings.Add(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeInformation.NamespaceName)));
        }

        CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit()
            .AddUsings(usings.ToArray())
            .AddMembers(classDeclaration);
        string sources = compilationUnit.NormalizeWhitespace().ToFullString();
        return sources;
    }


    private static string GenerateTestClassName(TypeInformation typeInformation)
    {
        return typeInformation.Name + TestClassPostfix;
    }

    private static ClassDeclarationSyntax GenerateClass(TypeInformation typeInformation)
    {
        var fields = new List<MemberDeclarationSyntax>();
        ConstructorInformation? constructor = FindLargestConstructor(typeInformation.Constructors);
        if (constructor is not null)
        {
            IDictionary<string, string> interfaceParametersNameTypeDictionary =
                DetermineInterfaceParameters(constructor.ParametersNameTypeDictionary);
            foreach ((string parameterName, string parameterType) in interfaceParametersNameTypeDictionary)
            {
                VariableDeclarationSyntax mockVariable =
                    GenerateVariable($"_{parameterName}", $"Mock<{parameterType}>");
                FieldDeclarationSyntax mockField = GenerateField(mockVariable);
                fields.Add(mockField);
            }
        }

        VariableDeclarationSyntax variableDependingOnTargetClass =
            GenerateVariable(DetermineVariableNameDependingOnTypeName(typeInformation.Name), typeInformation.InnerName);
        FieldDeclarationSyntax fieldDependingOnTargetClass = GenerateField(variableDependingOnTargetClass);
        fields.Add(fieldDependingOnTargetClass);

        var methods = new List<MemberDeclarationSyntax>();
        if (constructor is not null)
        {
            MethodDeclarationSyntax setUpMethodDeclaration = GenerateSetUpMethod(typeInformation.Name, constructor);
            methods.Add(setUpMethodDeclaration);
        }

        IEnumerable<MethodDeclarationSyntax> typeMethods =
            typeInformation.Methods.Select(method => GenerateMethod(typeInformation.Name, method));
        methods.AddRange(typeMethods);

        ClassDeclarationSyntax classDeclaration = SyntaxFactory
            .ClassDeclaration(GenerateTestClassName(typeInformation))
            .AddMembers(fields.ToArray())
            .AddMembers(methods.ToArray())
            .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(ClassAttribute));
        return classDeclaration;
    }

    private static ConstructorInformation? FindLargestConstructor(IEnumerable<ConstructorInformation> constructors)
    {
        ConstructorInformation? largestConstructor =
            constructors.MaxBy(constructor => constructor.ParametersNameTypeDictionary.Count);
        return largestConstructor;
    }

    private static IDictionary<string, string> DetermineNonInterfaceParameters(
        IDictionary<string, string> parameterNameTypeDictionary)
    {
        IDictionary<string, string> nonInterfaceParametersNameTypeDictionary = parameterNameTypeDictionary
            .Where(parameter => !parameter.Key.StartsWith('I'))
            .ToDictionary(parameter => parameter.Key, parameter => parameter.Value);
        return nonInterfaceParametersNameTypeDictionary;
    }

    private static IDictionary<string, string> DetermineInterfaceParameters(
        IDictionary<string, string> parameterNameTypeDictionary)
    {
        IDictionary<string, string> interfaceParametersNameTypeDictionary = parameterNameTypeDictionary
            .Where(parameter => parameter.Value.StartsWith('I'))
            .ToDictionary(parameter => parameter.Key, parameter => parameter.Value);
        return interfaceParametersNameTypeDictionary;
    }

    private static string ConvertParametersToStringRepresentation(IDictionary<string, string> parameters)
    {
        IList<string> parametersStrings =
            parameters.Select(pair => pair.Value.StartsWith('I') ? $"_{pair.Key}.Object" : $"{pair.Key}").ToList();
        string parametersString = string.Join(", ", parametersStrings);
        return parametersString;
    }

    private static string DetermineVariableNameDependingOnTypeName(string typeName)
    {
        string variableNameDependingOnTypeName = "_" + ReformatToStartWithLower(typeName);
        return variableNameDependingOnTypeName;
    }

    private static string ReformatToStartWithLower(string str)
    {
        return char.ToLower(str[0]) + str.Remove(0, 1);
    }

    private static StatementSyntax GenerateDefaultAssignStatement(string name, string typeName)
    {
        StatementSyntax statement = SyntaxFactory.ParseStatement($"var {name} = default({typeName});");
        return statement;
    }

    private static StatementSyntax GenerateExplicitAssignStatement(string name, string typeName,
                                                                   string parametersString = "")
    {
        StatementSyntax statement =
            SyntaxFactory.ParseStatement($"var {name} = new {typeName}({parametersString});");
        return statement;
    }

    private static StatementSyntax GenerateMethodCall(string resultVariableName, string methodName,
                                                      string invocationParametersString = "")
    {
        StatementSyntax methodInvocationStatement =
            SyntaxFactory.ParseStatement($"var {resultVariableName} = {methodName}({invocationParametersString});");
        return methodInvocationStatement;
    }

    private static StatementSyntax GenerateVoidMethodCall(string methodName,
                                                          string invocationParametersString = "")
    {
        StatementSyntax methodInvocationStatement =
            SyntaxFactory.ParseStatement($"{methodName}({invocationParametersString});");
        return methodInvocationStatement;
    }

    private static VariableDeclarationSyntax GenerateVariable(string name, string typeName)
    {
        TypeSyntax type = SyntaxFactory.ParseTypeName(typeName);
        VariableDeclaratorSyntax variableDeclarator = SyntaxFactory.VariableDeclarator(name);
        VariableDeclarationSyntax variableDeclaration =
            SyntaxFactory.VariableDeclaration(type).AddVariables(variableDeclarator);
        return variableDeclaration;
    }

    private static FieldDeclarationSyntax GenerateField(VariableDeclarationSyntax variableDeclaration)
    {
        FieldDeclarationSyntax fieldDeclaration =
            SyntaxFactory.FieldDeclaration(variableDeclaration).AddModifiers(PrivateModifier);
        return fieldDeclaration;
    }

    private static IEnumerable<StatementSyntax> GenerateArrangeSection(
        IDictionary<string, string> parameterNameTypeDictionary)
    {
        IDictionary<string, string> nonInterfaceParametersNameTypeDictionary =
            DetermineNonInterfaceParameters(parameterNameTypeDictionary);
        IEnumerable<StatementSyntax> assignStatements = nonInterfaceParametersNameTypeDictionary
            .Select(param => GenerateDefaultAssignStatement(param.Key, param.Value));
        return assignStatements;
    }

    private static IEnumerable<StatementSyntax> GenerateActSection(MethodInformation targetMethod, string typeName)
    {
        string targetMethodName = DetermineVariableNameDependingOnTypeName(typeName) + "." + targetMethod.Name;
        string invocationParametersString =
            ConvertParametersToStringRepresentation(targetMethod.ParametersNameTypeDictionary);
        StatementSyntax targetMethodInvocationStatement = targetMethod.ReturnType != typeof(void).Name
                                                              ? GenerateMethodCall("actual", targetMethodName,
                                                                  invocationParametersString)
                                                              : GenerateVoidMethodCall(
                                                                  targetMethodName, invocationParametersString);
        var statements = new List<StatementSyntax>
        {
            targetMethodInvocationStatement
        };
        return statements;
    }

    private static IEnumerable<StatementSyntax> GenerateAssertSection(string returnType)
    {
        StatementSyntax expectedAssignmentStatement = GenerateDefaultAssignStatement("expected", returnType);
        InvocationExpressionSyntax memberAccessExpression = GenerateMemberAccessExpression("Assert", "AreEqual");
        StatementSyntax expectedActualAssertStatement = SyntaxFactory.ExpressionStatement(
            memberAccessExpression.WithArgumentList(SyntaxFactory.ArgumentList()
                                                        .AddArguments(
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.IdentifierName("expected")),
                                                            SyntaxFactory.Argument(
                                                                SyntaxFactory.IdentifierName("actual")))));
        var statements = new List<StatementSyntax>
        {
            expectedAssignmentStatement, expectedActualAssertStatement
        };
        return statements;
    }

    private static InvocationExpressionSyntax GenerateMemberAccessExpression(string expression, string memberName)
    {
        InvocationExpressionSyntax memberAccessExpression = SyntaxFactory
            .InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                       SyntaxFactory.IdentifierName(expression),
                                                                       SyntaxFactory.IdentifierName(memberName)));
        return memberAccessExpression;
    }

    private static MethodDeclarationSyntax GenerateSetUpMethod(string typeName,
                                                               ConstructorInformation constructorInformation)
    {
        IDictionary<string, string> nonInterfaceParameters =
            DetermineNonInterfaceParameters(constructorInformation.ParametersNameTypeDictionary);
        List<StatementSyntax> statements = nonInterfaceParameters
            .Select(param => GenerateDefaultAssignStatement(param.Key, param.Value))
            .ToList();

        IDictionary<string, string> interfaceParameters =
            DetermineInterfaceParameters(constructorInformation.ParametersNameTypeDictionary);
        IEnumerable<StatementSyntax> nonInterfaceAssignmentStatements = interfaceParameters
            .Select(param => GenerateExplicitAssignStatement($"_{param.Key}", $"Mock<{param.Value}>"));
        statements.AddRange(nonInterfaceAssignmentStatements);

        string invocationParametersString =
            ConvertParametersToStringRepresentation(constructorInformation.ParametersNameTypeDictionary);
        StatementSyntax classInstanceAssignmentStatement =
            GenerateExplicitAssignStatement(DetermineVariableNameDependingOnTypeName(typeName),
                                            typeName, invocationParametersString);
        statements.Add(classInstanceAssignmentStatement);
        MethodDeclarationSyntax methodDeclaration = SyntaxFactory
            .MethodDeclaration(VoidReturnType, "SetUp")
            .AddModifiers(PublicModifier)
            .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(SetupAttribute))
            .WithBody(SyntaxFactory.Block(statements));
        return methodDeclaration;
    }

    private static MethodDeclarationSyntax GenerateMethod(string typeName, MethodInformation method)
    {
        var statements = new List<StatementSyntax>();
        statements.AddRange(GenerateArrangeSection(method.ParametersNameTypeDictionary));
        statements.AddRange(GenerateActSection(method, typeName));
        if (method.ReturnType != typeof(void).Name)
        {
            statements.AddRange(GenerateAssertSection(method.ReturnType));
        }

        statements.Add(GenerateFailStatement());
        MethodDeclarationSyntax methodDeclaration = SyntaxFactory.MethodDeclaration(VoidReturnType, method.Name)
            .AddModifiers(PublicModifier)
            .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(MethodAttribute))
            .WithBody(SyntaxFactory.Block(statements));
        return methodDeclaration;
    }

    private static StatementSyntax GenerateFailStatement()
    {
        ExpressionSyntax memberAccessExpression = SyntaxFactory
            .MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("Assert"),
                                    SyntaxFactory.IdentifierName("Fail"));
        ArgumentSyntax message = SyntaxFactory.Argument(
            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal("Generated")));
        ArgumentListSyntax invocationArgumentList = SyntaxFactory.ArgumentList().AddArguments(message);
        ExpressionSyntax failInvocationSyntax = SyntaxFactory.InvocationExpression(memberAccessExpression)
            .WithArgumentList(invocationArgumentList);
        StatementSyntax failStatement = SyntaxFactory.ExpressionStatement(failInvocationSyntax);
        return failStatement;
    }
}
