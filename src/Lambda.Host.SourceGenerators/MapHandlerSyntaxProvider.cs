using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lambda.Host.SourceGenerators.Extensions;
using Lambda.Host.SourceGenerators.Models;
using Lambda.Host.SourceGenerators.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lambda.Host.SourceGenerators;

internal static class MapHandlerSyntaxProvider
{
    /// <summary>
    ///     Determines whether the specified <paramref name="node" /> represents a valid invocation of the
    ///     method
    ///     identified by <see cref="GeneratorConstants.MapHandlerMethodName" />.
    /// </summary>
    /// <param name="node">The syntax node to evaluate.</param>
    /// <param name="cancellationToken">The cancellation token used to observe cancellation requests.</param>
    /// <returns>
    ///     <c>true</c> if the specified <paramref name="node" /> is an invocation of the MapHandler
    ///     method;
    ///     otherwise, <c>false</c>.
    /// </returns>
    internal static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
    {
        if (node is not InvocationExpressionSyntax invocation)
            return false;

        return invocation.Expression
            is MemberAccessExpressionSyntax
            {
                Name.Identifier.ValueText: GeneratorConstants.MapHandlerMethodName
            };
    }

    /// <summary>
    ///     Extracts a <see cref="MapHandlerInvocationInfo" /> object from the given syntax context
    ///     if the syntax node represents a valid MapHandler invocation.
    /// </summary>
    /// <param name="context">The context containing the syntax information and semantic model.</param>
    /// <param name="token">The cancellation token to observe cancellation requests.</param>
    /// <returns>
    ///     A <see cref="MapHandlerInvocationInfo" /> object containing details about the delegate if the
    ///     syntax
    ///     corresponds to a valid handler invocation; otherwise, <c>null</c>.
    /// </returns>
    internal static MapHandlerInvocationInfo? Transformer(
        GeneratorSyntaxContext context,
        CancellationToken token
    )
    {
        // validate that the method is from the LambdaApplication type
        if (context.Node is not InvocationExpressionSyntax invocationExpr)
            return null;

        var symbolInfo = ModelExtensions.GetSymbolInfo(context.SemanticModel, invocationExpr);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return null;

        // Check if it's from LambdaApplication
        if (methodSymbol.ContainingType?.Name != GeneratorConstants.StartupClassName)
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

        if (result is null)
            return null;

        return new MapHandlerInvocationInfo
        {
            LocationInfo = LocationInfo.CreateFrom(context.Node),
            DelegateInfo = updaters.Aggregate(result.Value, (current, updater) => updater(current)),
        };
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
            var castTypeInfo = ModelExtensions.GetTypeInfo(
                context.SemanticModel,
                castExpression.Type
            );

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
                        originalParam with
                        {
                            Type = castParam.Type.GetAsGlobal(),
                            LocationInfo = LocationInfo.CreateFrom(castParam),
                        }
                )
                .ToEquatableArray();

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
        var symbolInfo = ModelExtensions.GetSymbolInfo(context.SemanticModel, delegateExpression);

        // if a symbol is not found, try to find a candidate symbol as backup
        var symbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

        if (symbol is not IMethodSymbol methodSymbol)
            return null;

        var parameters = methodSymbol
            .Parameters.AsEnumerable()
            .Select(p => new ParameterInfo
            {
                ParameterName = p!.Name,
                Type = p.Type.GetAsGlobal(),
                LocationInfo = LocationInfo.CreateFrom(p),
                Attributes = p.GetAttributes()
                    .Select(a => new AttributeInfo(
                        a.ToString(),
                        a.ConstructorArguments.Where(aa => aa.Value is not null)
                            .Select(aa => aa.Value!.ToString())
                            .ToEquatableArray()
                    ))
                    .ToEquatableArray(),
            })
            .ToEquatableArray();

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
            .Select(p => new ParameterInfo(
                p!.Name,
                p.Type.GetAsGlobal(),
                LocationInfo.CreateFrom(p),
                p.GetAttributes()
                    .Select(a => new AttributeInfo(
                        a.ToString(),
                        a.ConstructorArguments.Where(aa => aa.Value is not null)
                            .Select(aa => aa.Value!.ToString())
                            .ToEquatableArray()
                    ))
                    .ToEquatableArray()
            ))
            .ToEquatableArray();

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
            ParenthesizedLambdaExpressionSyntax { ReturnType: { } syntax } => ModelExtensions
                .GetTypeInfo(sematicModel, syntax)
                .Type?.GetAsGlobal(syntax),

            // Handle implicit return type for expression lambda
            { Body: var expression and ExpressionSyntax } => sematicModel
                .GetTypeInfo(expression)
                .Type?.GetAsGlobal(),

            // Handle implicit return type for block lambda
            { Body: var block and BlockSyntax } => block
                .DescendantNodes()
                .OfType<ReturnStatementSyntax>()
                .FirstOrDefault(syntax => syntax.Expression is not null)
                ?.Transform(syntax =>
                    syntax.Expression is null
                        ? null
                        : ModelExtensions
                            .GetTypeInfo(sematicModel, syntax.Expression)
                            .Type?.GetAsGlobal()
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

        return new DelegateInfo(
            GetFileNamespace(context.Node, context.SemanticModel),
            isAsync,
            returnTypeName,
            parameters
        );
    }

    private delegate DelegateInfo Updater(DelegateInfo delegateInfo);
}
