using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lambda.Host.SourceGenerators;

[Generator]
public class MapHandlerIncrementalGenerator : IIncrementalGenerator
{
    private const string StartupClassName = "LambdaApplication";
    private const string MapHandlerMethodName = "MapHandler";

    private const string LambdaStartupServiceTemplateFile =
        "Templates/LambdaStartupService.scriban";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all MapHandler method calls with lambda analysis
        var mapHandlerCalls = context
            .SyntaxProvider.CreateSyntaxProvider(
                static (syntaxNode, cancellationToken) =>
                    IsMapHandlerCall(syntaxNode, cancellationToken),
                static (ctx, cancellationToken) => AnalyzeMapHandlerLambda(ctx, cancellationToken)
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        // Generate source when calls are found
        context.RegisterSourceOutput(
            mapHandlerCalls.Collect(),
            static (spc, calls) => GenerateLambdaReport(spc, calls)
        );
    }

    // Fast syntax check - look for MapHandler calls
    private static bool IsMapHandlerCall(SyntaxNode node, CancellationToken cancellationToken)
    {
        if (node is not InvocationExpressionSyntax invocation)
            return false;

        return invocation.Expression
            is MemberAccessExpressionSyntax { Name.Identifier.ValueText: MapHandlerMethodName };
    }

    // Analyze the lambda expression passed to MapHandler
    private static DelegateInfo? AnalyzeMapHandlerLambda(
        GeneratorSyntaxContext context,
        CancellationToken token
    )
    {
        // validate that the method is from the LambdaApplication type
        if (context.Node is not InvocationExpressionSyntax invocationExpr)
            return null;

        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocationExpr);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return null;

        // Check if it's from LambdaApplication
        if (methodSymbol.ContainingType?.Name != StartupClassName)
            return null;

        var firstArgument = invocationExpr.ArgumentList.Arguments[0];

        return firstArgument.Expression switch
        {
            // handle delegate expression
            IdentifierNameSyntax or MemberAccessExpressionSyntax => ExtractInfoFromDelegate(
                context,
                firstArgument.Expression
            ),

            // We can know that the lambda MUST be a ParenthesizedLambdaExpression as
            // SimpleLambdaExpression won't satisfy the Delegate type for MapHandler
            ParenthesizedLambdaExpressionSyntax lambda => ExtractInfoFromLambda(context, lambda),

            // check for cast expression
            CastExpressionSyntax castExpression => ExtractInfoFromCastLambda(
                context,
                castExpression
            ),

            _ => null,
        };
    }

    private static Func<DelegateInfo, DelegateInfo> UpdateTypesFromCast(
        GeneratorSyntaxContext context,
        CastExpressionSyntax castExpression
    ) =>
        (delegateInfo) =>
        {
            var castTypeInfo = context.SemanticModel.GetTypeInfo(castExpression.Type);

            if (castTypeInfo.Type is IErrorTypeSymbol)
                throw new InvalidOperationException(
                    $"Failed to resolve type info for {castTypeInfo.Type.ToDisplayString()}."
                );

            if (castTypeInfo.Type is not INamedTypeSymbol namedType)
                throw new InvalidOperationException(
                    $"Cast type must be a named delegate type, but got {castTypeInfo.Type?.ToDisplayString() ?? "null"}."
                );

            var invokeMethod = namedType
                .GetMembers("Invoke")
                .OfType<IMethodSymbol>()
                .FirstOrDefault();

            if (invokeMethod == null)
                throw new InvalidOperationException(
                    $"Cast type {namedType.ToDisplayString()} is not a valid delegate type (missing Invoke method)."
                );

            if (invokeMethod.Parameters.Length != delegateInfo.Parameters.Count)
                throw new InvalidOperationException(
                    $"Parameter count mismatch: cast delegate has {invokeMethod.Parameters.Length} parameters, "
                        + $"but existing delegate has {delegateInfo.Parameters.Count} parameters."
                );

            var updatedParameters = invokeMethod
                .Parameters.Zip(
                    delegateInfo.Parameters,
                    (castParam, originalParam) =>
                        new ParameterInfo
                        {
                            ParameterName = originalParam.ParameterName,
                            Type = castParam.Type.GetAsGlobal(),
                            Attributes = originalParam.Attributes,
                        }
                )
                .ToList();

            return new DelegateInfo
            {
                ResponseType = invokeMethod.ReturnType.GetAsGlobal(),
                Namespace = delegateInfo.Namespace,
                IsAsync = invokeMethod.IsAsync,
                Parameters = updatedParameters,
            };
        };

    private static string GetFileNamespace(SyntaxNode node, SemanticModel semanticModel)
    {
        // First try to find explicit namespace declaration
        var namespaceDeclaration = node.Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault();

        if (namespaceDeclaration != null)
            return namespaceDeclaration.Name.ToString();

        // For top-level statements, get the default namespace from compilation
        var compilation = semanticModel.Compilation;
        return compilation.Assembly.Name; // This will be "Lambda.Host.Example.HelloWorld"
    }

    private static DelegateInfo? ExtractInfoFromDelegate(
        GeneratorSyntaxContext context,
        ExpressionSyntax delegateExpression
    )
    {
        var symbolInfo = context.SemanticModel.GetSymbolInfo(delegateExpression);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            // Handle method group case
            if (
                symbolInfo.CandidateSymbols.Length > 0
                && symbolInfo.CandidateSymbols[0] is IMethodSymbol candidateMethod
            )
                methodSymbol = candidateMethod;
            else
                return null;
        }

        var parameters = methodSymbol
            .Parameters.AsEnumerable()
            .Select(p =>
            {
                return new ParameterInfo
                {
                    ParameterName = p!.Name,
                    Type = p.Type.GetAsGlobal(),
                    Attributes = p.GetAttributes()
                        .Select(a => new AttributeInfo
                        {
                            Type = a.ToString(),
                            Arguments = a
                                .ConstructorArguments.Select(aa => aa.Value?.ToString())
                                .Where(aa => aa is not null)
                                .ToList()!,
                        })
                        .ToList(),
                };
            })
            .ToList();

        return new DelegateInfo
        {
            ResponseType = methodSymbol.ReturnType.GetAsGlobal(),
            Namespace = GetFileNamespace(context.Node, context.SemanticModel),
            IsAsync = methodSymbol.IsAsync,
            Parameters = parameters,
        };
    }

    private static DelegateInfo? ExtractInfoFromCastLambda(
        GeneratorSyntaxContext context,
        CastExpressionSyntax castExpression
    )
    {
        var castTypeInfo = context.SemanticModel.GetTypeInfo(castExpression.Type);

        if (castTypeInfo.Type is IErrorTypeSymbol)
            throw new InvalidOperationException(
                $"Failed to resolve type info for {castTypeInfo.Type.ToDisplayString()}."
            );

        if (castTypeInfo.Type is not INamedTypeSymbol namedType)
            return null;

        var invokeMethod = namedType.GetMembers("Invoke").OfType<IMethodSymbol>().FirstOrDefault();

        // The cast expression contains a parenthesized expression, which contains the lambda
        if (castExpression.Expression is not ParenthesizedExpressionSyntax parenthesizedExpr)
            return null;

        // Now get the lambda from inside the parentheses
        if (parenthesizedExpr.Expression is not ParenthesizedLambdaExpressionSyntax innerLambda)
            return null;

        var symbolInfo = context.SemanticModel.GetSymbolInfo(innerLambda);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol2)
        {
            // Handle method group case
            if (
                symbolInfo.CandidateSymbols.Length > 0
                && symbolInfo.CandidateSymbols[0] is IMethodSymbol candidateMethod
            )
                methodSymbol2 = candidateMethod;
            else
                return null;
        }

        var parameters =
            invokeMethod
                ?.Parameters.Zip<IParameterSymbol?, IParameterSymbol, ParameterInfo?>(
                    methodSymbol2.Parameters,
                    (t, p) =>
                        t is not null && p is not null
                            ? new ParameterInfo
                            {
                                ParameterName = p.Name,
                                Type = t.Type.GetAsGlobal(),
                                Attributes = p.GetAttributes()
                                    .Select(a => new AttributeInfo
                                    {
                                        Type = a.ToString(),
                                        Arguments = a
                                            .ConstructorArguments.Select(aa => aa.Value?.ToString())
                                            .Where(aa => aa is not null)
                                            .ToList()!,
                                    })
                                    .ToList(),
                            }
                            : null
                )
                .Where(p => p is not null)
                .ToList() ?? [];

        return new DelegateInfo
        {
            ResponseType = invokeMethod?.ReturnType.GetAsGlobal() ?? TypeConstants.Void,
            Namespace = GetFileNamespace(context.Node, context.SemanticModel),
            IsAsync = invokeMethod?.IsAsync ?? false,
            Parameters = parameters!,
        };
    }

    private static DelegateInfo? ExtractInfoFromLambda(
        GeneratorSyntaxContext context,
        ParenthesizedLambdaExpressionSyntax lambdaExpression
    )
    {
        var sematicModel = context.SemanticModel;
        var lambdaTypeInfo = sematicModel.GetTypeInfo(lambdaExpression);
        var delegateType = lambdaTypeInfo.ConvertedType as INamedTypeSymbol;

        // extract parameter information
        var parameters = lambdaExpression
            .ParameterList.Parameters.AsEnumerable()
            .Select(p => sematicModel.GetDeclaredSymbol(p))
            .Where(p => p is not null)
            .Select(p =>
            {
                return new ParameterInfo
                {
                    ParameterName = p!.Name,
                    Type = p.Type.GetAsGlobal(),
                    Attributes = p.GetAttributes()
                        .Select(a => new AttributeInfo
                        {
                            Type = a.ToString(),
                            Arguments = a
                                .ConstructorArguments.Select(aa => aa.Value?.ToString())
                                .Where(aa => aa is not null)
                                .ToList()!,
                        })
                        .ToList(),
                };
            })
            .ToList();

        var isAsync = lambdaExpression.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword);

        // Hierarchy for determining lambda return type.
        //
        // 1. type conversion (not handled here)
        // 2. explicit return type
        // 3. implicit return type in expression body
        // 4. implicit return type in block body
        // 5. default void (or Task if async)
        var returnType = lambdaExpression switch
        {
            // check for explicit return type
            { ReturnType: var syntax } when syntax is not null => sematicModel
                .GetTypeInfo(syntax)
                .Type?.GetAsGlobal(syntax),

            // Handle implicit return type for expression lambda
            { Body: var expression } when expression is ExpressionSyntax => sematicModel
                .GetTypeInfo(expression)
                .Type?.GetAsGlobal(),

            // Handle implicit return type for block lambda
            { Body: var block } when block is BlockSyntax => block
                .DescendantNodes()
                .OfType<ReturnStatementSyntax>()
                .FirstOrDefault(syntax => syntax.Expression is not null)
                ?.Transform(syntax =>
                    syntax.Expression is null
                        ? null
                        : sematicModel.GetTypeInfo(syntax.Expression).Type?.GetAsGlobal()
                ),

            // Default to void if no return type is found
            _ => null,
        };

        var returnTypeName = (ReturnType: returnType, IsAsync: isAsync) switch
        {
            (null, true) => TypeConstants.Task,
            (null, false) => TypeConstants.Void,
            (TypeConstants.Task, true) => TypeConstants.Task,
            (var type, true) => $"{TypeConstants.Task}<{type}>",
            var (type, _) => type,
        };

        return new DelegateInfo
        {
            ResponseType = returnTypeName,
            Namespace = GetFileNamespace(context.Node, context.SemanticModel),
            IsAsync = isAsync,
            Parameters = parameters,
        };
    }

    private static List<ParameterInfo> ExtractLambdaParameters(
        GeneratorSyntaxContext context,
        ParenthesizedLambdaExpressionSyntax lambdaExpression
    )
    {
        var sematicModel = context.SemanticModel;

        // extract parameter information
        return lambdaExpression
            .ParameterList.Parameters.AsEnumerable()
            .Select(p => sematicModel.GetDeclaredSymbol(p))
            .Where(p => p is not null)
            .Select(p =>
            {
                return new ParameterInfo
                {
                    ParameterName = p!.Name,
                    Type = p.Type.GetAsGlobal(),
                    Attributes = p.GetAttributes()
                        .Select(a => new AttributeInfo
                        {
                            Type = a.ToString(),
                            Arguments = a
                                .ConstructorArguments.Select(aa => aa.Value?.ToString())
                                .Where(aa => aa is not null)
                                .ToList()!,
                        })
                        .ToList(),
                };
            })
            .ToList();
    }

    private static (string returnType, bool isAsync) ExtractLambdaReturnType(
        GeneratorSyntaxContext context,
        ParenthesizedLambdaExpressionSyntax lambdaExpression
    )
    {
        var sematicModel = context.SemanticModel;

        var isAsync = lambdaExpression.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword);

        // Hierarchy for determining lambda return type.
        //
        // 1. type conversion (not handled here)
        // 2. explicit return type
        // 3. implicit return type in expression body
        // 4. implicit return type in block body
        // 5. default void (or Task if async)
        var returnType = lambdaExpression switch
        {
            // check for explicit return type
            { ReturnType: var syntax } when syntax is not null => sematicModel
                .GetTypeInfo(syntax)
                .Type?.GetAsGlobal(syntax),

            // Handle implicit return type for expression lambda
            { Body: var expression } when expression is ExpressionSyntax => sematicModel
                .GetTypeInfo(expression)
                .Type?.GetAsGlobal(),

            // Handle implicit return type for block lambda
            { Body: var block } when block is BlockSyntax => block
                .DescendantNodes()
                .OfType<ReturnStatementSyntax>()
                .FirstOrDefault(syntax => syntax.Expression is not null)
                ?.Transform(syntax =>
                    syntax.Expression is null
                        ? null
                        : sematicModel.GetTypeInfo(syntax.Expression).Type?.GetAsGlobal()
                ),

            // Default to void if no return type is found
            _ => null,
        };

        var returnTypeName = (ReturnType: returnType, IsAsync: isAsync) switch
        {
            (null, true) => TypeConstants.Task,
            (null, false) => TypeConstants.Void,
            (TypeConstants.Task, true) => TypeConstants.Task,
            (var type, true) => $"{TypeConstants.Task}<{type}>",
            var (type, _) => type,
        };

        return (returnTypeName, isAsync);
    }

    private static void GenerateLambdaReport(
        SourceProductionContext context,
        ImmutableArray<DelegateInfo> delegateInfos
    )
    {
        if (delegateInfos.Length == 0)
            return;

        var delegateInfo = delegateInfos.First();

        var delegateArguments = (delegateInfo.Parameters.Select(p => p.Type) ?? [])
            .Concat(
                new[] { delegateInfo?.ResponseType }.Where(t =>
                    t != null && t != TypeConstants.Void
                )
            )
            .ToList();

        var classFields = delegateInfo
            .Parameters.Where(p =>
                p.Attributes.All(a => a.Type != AttributeConstants.Request)
                && p.Type != TypeConstants.ILambdaContext
            )
            .Select(p => new
            {
                attributes = p.Attributes.Select(a => a.Type).ToList(),
                keyed_service_key = p
                    .Attributes.Where(a =>
                        a?.Type?.StartsWith(AttributeConstants.FromKeyedService) ?? false
                    )
                    .Select(a => a.Arguments.FirstOrDefault())
                    .FirstOrDefault(),
                name = p.ParameterName.ToCamelCase(),
                type = p.Type,
            })
            .ToList();

        var handlerArgs =
            delegateInfo.Parameters.Select(p => p.ParameterName.ToCamelCase()).ToList() ?? [];

        var lambdaParams =
            delegateInfo
                .Parameters.Where(p =>
                    p.Attributes.Any(a => a.Type == AttributeConstants.Request)
                    || p.Type == TypeConstants.ILambdaContext
                )
                .OrderBy(p => p.Type == TypeConstants.ILambdaContext ? 1 : 0)
                .Select(p => p.Type + " " + p.ParameterName.ToCamelCase())
                .ToList() ?? [];

        // 1. if Action -> no return
        // 3. if Func + Task return type + async -> no return
        // 2. if Func + Task return type -> return value
        // 4. if Func + non-Task return type -> return value
        var hasReturnValue = delegateInfo switch
        {
            { DelegateType: "Action" } => false,
            { DelegateType: "Func", IsAsync: true, ResponseType: TypeConstants.Task } => false,
            _ => true,
        };

        var model = new
        {
            @namespace = delegateInfo.Namespace,
            service = "LambdaStartupService",
            fields = classFields,
            delegate_type = delegateInfo.DelegateType,
            delegate_args = delegateArguments,
            handler_args = handlerArgs,
            lambda_params = lambdaParams,
            is_lambda_async = delegateInfo?.IsAsync ?? false,
            has_return_value = hasReturnValue,
        };

        var template = TemplateHelper.LoadTemplate(LambdaStartupServiceTemplateFile);

        var outCode = template.Render(model);

        context.AddSource("LambdaStartup.g.cs", outCode);
    }
}

internal sealed class DelegateInfo
{
    internal required string? ResponseType { get; set; } = TypeConstants.Void;
    internal required string? Namespace { get; set; } = null;
    internal required bool IsAsync { get; set; } = false;

    internal string DelegateType => ResponseType == TypeConstants.Void ? "Action" : "Func";
    internal List<ParameterInfo> Parameters { get; set; } = [];
}

internal sealed class ParameterInfo
{
    internal required string? ParameterName { get; set; } = null;
    internal required string? Type { get; set; } = null;
    internal List<AttributeInfo> Attributes { get; set; } = [];
}

internal sealed class AttributeInfo
{
    internal required string? Type { get; set; } = null;
    internal List<string> Arguments { get; set; } = [];
}
