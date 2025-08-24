using System.Collections;
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

        // setup list of mutator functions
        List<Updater> updaters = [];

        var handler = invocationExpr.ArgumentList.Arguments.ElementAtOrDefault(0)?.Expression;

        // if we are dealing with a cast expression, set up a mutator to update the delegate type
        if (handler is CastExpressionSyntax castExpression)
        {
            handler = GetDelegateFromCast(castExpression);
            if (handler is null)
                return null;

            updaters.Add(UpdateTypesFromCast(context, castExpression));
        }

        var result = handler switch
        {
            IdentifierNameSyntax or MemberAccessExpressionSyntax => ExtractInfoFromDelegate(
                context,
                handler
            ),

            LambdaExpressionSyntax lambda => ExtractInfoFromLambda(context, lambda),

            _ => null,
        };

        return updaters.Aggregate(result, (current, updater) => updater(current!));
    }

    private static ExpressionSyntax? GetDelegateFromCast(CastExpressionSyntax castExpression)
    {
        // must have at least 2 children -> expression at index 1, cast at index 0
        var expression = castExpression.ChildNodes().ElementAtOrDefault(1);
        if (expression is null)
            return null;

        // unwrap parenthesized expressions
        while (expression is ParenthesizedExpressionSyntax parenthesizedExpression)
            expression = parenthesizedExpression.Expression;

        return expression switch
        {
            // top level static method - e.g. (Func<Int32>)Handler
            IdentifierNameSyntax identifier => identifier,

            // static method on a class - e.g. (Func<Int32>)MyClass.Handler
            MemberAccessExpressionSyntax memberAccess => memberAccess,

            // parenthesized lambda expression - e.g. (Func<Int32>)() => 1
            ParenthesizedLambdaExpressionSyntax parenthesizedLambda => parenthesizedLambda,

            // simple lambda expression - e.g. (Func<Int32, Int32>)x => x + 1
            SimpleLambdaExpressionSyntax simpleLambda => simpleLambda,

            // default, not a supported delegate type
            _ => null,
        };
    }

    private static Updater UpdateTypesFromCast(
        GeneratorSyntaxContext context,
        CastExpressionSyntax castExpression
    ) =>
        delegateInfo =>
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

        // if a symbol is not found, try to find a candidate symbol as backup
        var symbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

        if (symbol is not IMethodSymbol methodSymbol)
            return null;

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

    private static DelegateInfo ExtractInfoFromLambda(
        GeneratorSyntaxContext context,
        LambdaExpressionSyntax lambdaExpression
    )
    {
        var sematicModel = context.SemanticModel;

        var parameterSyntaxes = lambdaExpression switch
        {
            SimpleLambdaExpressionSyntax simpleLambda => new[] { simpleLambda.Parameter }.Where(p =>
                p != null
            ),
            ParenthesizedLambdaExpressionSyntax parenthesizedLambda =>
                parenthesizedLambda.ParameterList.Parameters.AsEnumerable(),
            _ => [],
        };

        // extract parameter information
        var parameters = parameterSyntaxes
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
            ParenthesizedLambdaExpressionSyntax { ReturnType: var syntax }
                when syntax is not null => sematicModel
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

    private delegate DelegateInfo Updater(DelegateInfo delegateInfo);
}

internal sealed class DelegateInfo
{
    internal required string? ResponseType { get; set; } = TypeConstants.Void;
    internal required string? Namespace { get; set; }
    internal required bool IsAsync { get; set; }

    internal string DelegateType => ResponseType == TypeConstants.Void ? "Action" : "Func";
    internal List<ParameterInfo> Parameters { get; set; } = [];
}

internal sealed class ParameterInfo
{
    internal required string? ParameterName { get; set; }
    internal required string? Type { get; set; }
    internal List<AttributeInfo> Attributes { get; set; } = [];
}

internal sealed class AttributeInfo
{
    internal required string? Type { get; set; }
    internal List<string> Arguments { get; set; } = [];
}
