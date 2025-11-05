using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AwsLambda.Host.SourceGenerators.Extensions;
using AwsLambda.Host.SourceGenerators.Models;
using AwsLambda.Host.SourceGenerators.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AwsLambda.Host.SourceGenerators;

internal static class DelegateInfoExtractorExtensions
{
    internal static DelegateInfo? ExtractDelegateInfo(
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
                            Type = castParam.Type.GetAsGlobal(),
                            LocationInfo = LocationInfo.CreateFrom(castParam),
                        }
                )
                .ToEquatableArray();

            // get the fully qualified type that may be wrapped in Task or ValueTask.
            var fullResponseType = invokeMethod.ReturnType.GetAsGlobal();

            // get the unwrapped type if it is wrapped in Task or ValueTask.
            var unwrappedResponseType = (
                (INamedTypeSymbol)invokeMethod.ReturnType
            ).UnwrapTypeFromTask();

            // determine if the delegate is returning awaitable value
            var isAwaitable =
                fullResponseType != TypeConstants.Void
                && (invokeMethod.IsAsync || invokeMethod.ReturnType.IsTypeAwaitable());

            return new DelegateInfo(
                fullResponseType,
                unwrappedResponseType,
                updatedParameters,
                isAwaitable,
                delegateInfo.IsAsync
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

        // get the unwrapped type if it is wrapped in Task or ValueTask.
        var unwrappedResponseType = (
            (INamedTypeSymbol)methodSymbol.ReturnType
        ).UnwrapTypeFromTask();

        // determine if the delegate is returning awaitable value
        var isAwaitable =
            fullResponseType != TypeConstants.Void
            && (methodSymbol.IsAsync || methodSymbol.ReturnType.IsTypeAwaitable());

        return new DelegateInfo(
            fullResponseType,
            unwrappedResponseType,
            parameters,
            isAwaitable,
            methodSymbol.IsAsync
        );
    }

    private static string? UnwrapTypeFromTask(
        this ITypeSymbol typeSymbol,
        TypeSyntax? syntax = null
    )
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
            return typeSymbol.GetAsGlobal(syntax);

        if (!namedTypeSymbol.IsTask() && !namedTypeSymbol.IsValueTask())
            return typeSymbol.GetAsGlobal(syntax);

        // if not generic Task or ValueTask, return null as no wrapped return value
        if (!namedTypeSymbol.IsGenericType || namedTypeSymbol.TypeArguments.Length == 0)
            return null;

        return namedTypeSymbol.TypeArguments.First().GetAsGlobal(syntax);
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
        var (returnType, unwrappedResponseType, responseType) = lambdaExpression switch
        {
            // check for explicit return type
            ParenthesizedLambdaExpressionSyntax { ReturnType: { } syntax } => ModelExtensions
                .GetTypeInfo(sematicModel, syntax, cancellationToken)
                .Type?.Transform(t => (t, t.UnwrapTypeFromTask(syntax), t.GetAsGlobal(syntax)))
            ?? (null, null, null)!,

            // Handle implicit return type for expression lambda
            { Body: var expression and ExpressionSyntax } => sematicModel
                .GetTypeInfo(expression, cancellationToken)
                .Type?.Transform(t => (t, t.UnwrapTypeFromTask(), t.GetAsGlobal()))
            ?? (null, null, null)!,

            // Handle implicit return type for block lambda
            { Body: var block and BlockSyntax } => block
                .DescendantNodes()
                .OfType<ReturnStatementSyntax>()
                .FirstOrDefault(syntax => syntax.Expression is not null)
                ?.Transform(syntax =>
                    syntax.Expression is null
                        ? (null, null, null)
                        : ModelExtensions
                            .GetTypeInfo(sematicModel, syntax.Expression, cancellationToken)
                            .Type?.Transform(t => (t, t.UnwrapTypeFromTask(), t.GetAsGlobal()))
                        ?? (null, null, null)!
                )
            ?? (null, null, null),

            // Default to void if no return type is found
            _ => (null, null, null),
        };

        // determine if the lambda is async by checking kind
        var isAsync = lambdaExpression.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword);

        // the full return type for use in function signatures.
        var fullResponseType = (ReturnType: responseType, IsAsync: isAsync) switch
        {
            (null, true) => TypeConstants.Task,
            (null, false) => TypeConstants.Void,
            (TypeConstants.Void, _) => TypeConstants.Void,
            (TypeConstants.Task, _) => TypeConstants.Task,
            (TypeConstants.ValueTask, _) => TypeConstants.ValueTask,
            var (type, _) when type.StartsWith(TypeConstants.Task) => type,
            var (type, _) when type.StartsWith(TypeConstants.ValueTask) => type,
            (var type, true) => $"{TypeConstants.Task}<{type}>",
            (_, _) => responseType,
        };

        // determine if the delegate is returning awaitable value
        var isAwaitable =
            fullResponseType != TypeConstants.Void
            && (isAsync || (returnType?.IsTypeAwaitable() ?? false));

        return new DelegateInfo(
            fullResponseType,
            unwrappedResponseType,
            parameters,
            isAwaitable,
            isAsync
        );
    }

    private static bool IsTypeAwaitable(this ITypeSymbol typeSymbol) =>
        typeSymbol.IsTask() || typeSymbol.IsValueTask();

    private static bool IsTask(this ITypeSymbol typeSymbol) =>
        typeSymbol.Name == "Task"
        && typeSymbol.ContainingNamespace?.ToDisplayString() == "System.Threading.Tasks";

    private static bool IsValueTask(this ITypeSymbol typeSymbol) =>
        typeSymbol.Name == "ValueTask"
        && typeSymbol.ContainingNamespace?.ToDisplayString() == "System.Threading.Tasks";

    private delegate DelegateInfo Updater(
        DelegateInfo delegateInfo,
        CancellationToken cancellationToken
    );
}
