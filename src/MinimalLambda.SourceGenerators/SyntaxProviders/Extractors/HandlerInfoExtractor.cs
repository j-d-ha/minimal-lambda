using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using MinimalLambda.SourceGenerators.Extensions;
using MinimalLambda.SourceGenerators.Models;
using MinimalLambda.SourceGenerators.Types;
using TypeInfo = MinimalLambda.SourceGenerators.Models.TypeInfo;

namespace MinimalLambda.SourceGenerators;

using TypeInfo = TypeInfo;

internal static class HandlerInfoExtractor
{
    internal static bool Predicate(SyntaxNode node, params string[] methodNames) =>
        !node.IsGeneratedFile()
        && node.TryGetMethodName(out var name)
        && methodNames.Contains(name);

    internal static HigherOrderMethodInfo? Transformer(
        GeneratorSyntaxContext context,
        Func<DelegateInfo, bool> delegateFilter,
        CancellationToken cancellationToken
    )
    {
        var operation = context.SemanticModel.GetOperation(context.Node, cancellationToken);

        if (
            operation
                is not IInvocationOperation
                {
                    TargetMethod.ContainingNamespace:
                    {
                        Name: "Builder",
                        ContainingNamespace:
                        { Name: "MinimalLambda", ContainingNamespace.IsGlobalNamespace: true },
                    },
                } targetOperation
            || targetOperation.TargetMethod.ContainingAssembly.Name != "MinimalLambda"
        )
            return null;

        if (context.Node is not InvocationExpressionSyntax invocationExpr)
            return null;

        var handler = invocationExpr.ArgumentList.Arguments.ElementAtOrDefault(0)?.Expression;

        var delegateInfo = handler?.ExtractDelegateInfo(context, cancellationToken);
        if (delegateInfo is null)
            return null;

        // filter out non-generic shutdown method calls
        if (delegateFilter(delegateInfo.Value))
            return null;

        // get method arguments
        var argumentInfos = targetOperation
            .Arguments.Select(argument =>
            {
                var typeAsGlobal = argument.Value.Type?.GetAsGlobal();
                var parameterName = argument.Parameter?.Name;

                return new ArgumentInfo(typeAsGlobal, parameterName);
            })
            .ToImmutableArray();

        // get interceptable location
        var interceptableLocation = context.SemanticModel.GetInterceptableLocation(
            invocationExpr,
            cancellationToken
        )!;

        return new HigherOrderMethodInfo(
            targetOperation.TargetMethod.Name,
            LocationInfo: context.Node.CreateLocationInfo(),
            DelegateInfo: delegateInfo.Value,
            InterceptableLocationInfo: InterceptableLocationInfo.CreateFrom(interceptableLocation),
            ArgumentsInfos: argumentInfos
        );
    }

    private static DelegateInfo? ExtractDelegateInfo(
        this ExpressionSyntax handler,
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        // setup list of mutator functions
        List<Updater> updaters = [];

        // if we are dealing with a cast expression, set up a mutator to update the delegate type
        if (handler is CastExpressionSyntax castExpression)
        {
            var del = GetDelegateFromCast(castExpression, cancellationToken);
            if (del is null)
                return null;

            handler = del;

            updaters.Add(UpdateTypesFromCast(context, castExpression));
        }

        var result = handler switch
        {
            IdentifierNameSyntax or MemberAccessExpressionSyntax => ExtractInfoFromDelegate(
                context,
                handler,
                cancellationToken
            ),

            LambdaExpressionSyntax lambda => ExtractInfoFromLambda(
                context,
                lambda,
                cancellationToken
            ),

            _ => null,
        };

        if (result is null)
            return null;

        return updaters.Aggregate(
            result.Value,
            (current, updater) => updater(current, cancellationToken)
        );
    }

    private static ExpressionSyntax? GetDelegateFromCast(
        CastExpressionSyntax castExpression,
        CancellationToken cancellationToken
    )
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
        (delegateInfo, cancellationToken) =>
        {
            var castTypeInfo = ModelExtensions.GetTypeInfo(
                context.SemanticModel,
                castExpression.Type,
                cancellationToken
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
                            TypeInfo = TypeInfo.Create(castParam.Type),
                            LocationInfo = castParam.CreateLocationInfo(),
                        }
                )
                .ToEquatableArray();

            // get the fully qualified type that may be wrapped in Task or ValueTask.
            var fullResponseType = invokeMethod.ReturnType.GetAsGlobal();

            // determine if the delegate is returning awaitable value
            var isAwaitable =
                fullResponseType != TypeConstants.Void
                && (invokeMethod.IsAsync || invokeMethod.ReturnType.IsTypeAwaitable());

            // get response type TypeInfo
            var responseTypeInfo = TypeInfo.Create(invokeMethod.ReturnType);

            return new DelegateInfo(
                updatedParameters,
                isAwaitable,
                delegateInfo.IsAsync,
                responseTypeInfo
            );
        };

    private static DelegateInfo? ExtractInfoFromDelegate(
        GeneratorSyntaxContext context,
        ExpressionSyntax delegateExpression,
        CancellationToken cancellationToken
    )
    {
        var symbolInfo = ModelExtensions.GetSymbolInfo(
            context.SemanticModel,
            delegateExpression,
            cancellationToken
        );

        // if a symbol is not found, try to find a candidate symbol as backup
        var symbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

        if (symbol is not IMethodSymbol methodSymbol)
            return null;

        var parameters = methodSymbol
            .Parameters.AsEnumerable()
            .Select(ParameterInfo.Create)
            .ToEquatableArray();

        // get the fully qualified type that may be wrapped in Task or ValueTask.
        var fullResponseType = methodSymbol.ReturnType.GetAsGlobal();

        // determine if the delegate is returning awaitable value
        var isAwaitable =
            fullResponseType != TypeConstants.Void
            && (methodSymbol.IsAsync || methodSymbol.ReturnType.IsTypeAwaitable());

        // get response type TypeInfo
        var responseTypeInfo = TypeInfo.Create(methodSymbol.ReturnType);

        return new DelegateInfo(parameters, isAwaitable, methodSymbol.IsAsync, responseTypeInfo);
    }

    private static DelegateInfo ExtractInfoFromLambda(
        GeneratorSyntaxContext context,
        LambdaExpressionSyntax lambdaExpression,
        CancellationToken cancellationToken
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
            .Select(p => sematicModel.GetDeclaredSymbol(p, cancellationToken))
            .Where(p => p is not null)
            .Select(ParameterInfo.Create!)
            .ToEquatableArray();

        // Hierarchy for determining lambda return type.
        //
        // 1. type conversion (not handled here)
        // 2. explicit return type
        // 3. implicit return type in expression body
        // 4. implicit return type in block body
        // 5. default void (or Task if async)
        var (returnType, returnTypeSyntax) = lambdaExpression switch
        {
            // check for explicit return type
            ParenthesizedLambdaExpressionSyntax { ReturnType: { } syntax } => ModelExtensions
                .GetTypeInfo(sematicModel, syntax, cancellationToken)
                .Type
                is { } type
                ? (type, syntax)
                : (null, null),

            // Handle implicit return type for expression lambda
            { Body: var expression and ExpressionSyntax } => (
                sematicModel.GetTypeInfo(expression, cancellationToken).Type,
                null
            ),

            // Handle implicit return type for block lambda
            { Body: var block and BlockSyntax } => block
                .DescendantNodes()
                .OfType<ReturnStatementSyntax>()
                .FirstOrDefault(s => s.Expression is not null)
                ?.Expression
                is { } expr
                ? (sematicModel.GetTypeInfo(expr, cancellationToken).Type, null)
                : (null, null),

            // Default to void if no return type is found
            _ => (null, null),
        };

        // get response type TypeInfo
        TypeInfo? responseTypeInfo = returnType is not null
            ? TypeInfo.Create(returnType, returnTypeSyntax)
            : null;

        // determine if the lambda is async by checking kind
        var isAsync = lambdaExpression.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword);

        // the full return type for use in function signatures.
        var fullResponseType = (
            ReturnType: responseTypeInfo?.FullyQualifiedType,
            IsAsync: isAsync
        ) switch
        {
            (null, true) => TypeConstants.Task,
            (null, false) => TypeConstants.Void,
            (TypeConstants.Void, _) => TypeConstants.Void,
            (TypeConstants.Task, _) => TypeConstants.Task,
            (TypeConstants.ValueTask, _) => TypeConstants.ValueTask,
            var (type, _) when type.StartsWith(TypeConstants.Task) => type,
            var (type, _) when type.StartsWith(TypeConstants.ValueTask) => type,
            (var type, true) => $"{TypeConstants.Task}<{type}>",
            (_, _) => responseTypeInfo.Value.FullyQualifiedType,
        };

        var updatedResponseType = responseTypeInfo is { } info
            ? info with
            {
                FullyQualifiedType = fullResponseType,
            }
            : TypeInfo.CreateFullyQualifiedType(fullResponseType);

        // determine if the delegate is returning awaitable value
        var isAwaitable =
            fullResponseType != TypeConstants.Void
            && (isAsync || (returnType?.IsTypeAwaitable() ?? false));

        return new DelegateInfo(parameters, isAwaitable, isAsync, updatedResponseType);
    }

    private delegate DelegateInfo Updater(
        DelegateInfo delegateInfo,
        CancellationToken cancellationToken
    );
}
