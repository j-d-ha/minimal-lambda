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
            handler = GetDelegateFromCast(castExpression, cancellationToken);
            if (handler is null)
                return null;

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

            return new DelegateInfo(invokeMethod.ReturnType.GetAsGlobal(), updatedParameters);
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
            .Select(p =>
            {
                var type = p.Type.GetAsGlobal();
                var location = LocationInfo.CreateFrom(p);
                var (source, key) = p.GetAttributes().GetSourceFromAttribute(type);

                return new ParameterInfo(type, location, source, key);
            })
            .ToEquatableArray();

        return new DelegateInfo(methodSymbol.ReturnType.GetAsGlobal(), parameters);
    }

    private static (ParameterSource Source, string? KeyedServiceKey) GetSourceFromAttribute(
        this IEnumerable<AttributeData> attributes,
        string type
    )
    {
        // try and extract source from attributes
        foreach (var attribute in attributes)
            switch (attribute.AttributeClass?.ToString())
            {
                case AttributeConstants.EventAttribute:
                    return (ParameterSource.Event, null);

                case AttributeConstants.FromKeyedService:
                    var key = attribute
                        .ConstructorArguments.Where(a => a.Value is not null)
                        .Select(a => a.Value!.ToString())
                        .SingleOrDefault();
                    return (ParameterSource.KeyedService, key);
            }

        // fallback to get source from type
        return type switch
        {
            TypeConstants.CancellationToken => (ParameterSource.ContextCancellation, null),
            TypeConstants.ILambdaContext => (ParameterSource.Context, null),
            TypeConstants.ILambdaHostContext => (ParameterSource.Context, null),
            _ => (ParameterSource.Service, null),
        };
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
            .Select(p =>
            {
                var type = p.Type.GetAsGlobal();
                var location = LocationInfo.CreateFrom(p);
                var (source, key) = p.GetAttributes().GetSourceFromAttribute(type);

                return new ParameterInfo(type, location, source, key);
            })
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
                .GetTypeInfo(sematicModel, syntax, cancellationToken)
                .Type?.GetAsGlobal(syntax),

            // Handle implicit return type for expression lambda
            { Body: var expression and ExpressionSyntax } => sematicModel
                .GetTypeInfo(expression, cancellationToken)
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
                            .GetTypeInfo(sematicModel, syntax.Expression, cancellationToken)
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

        return new DelegateInfo(returnTypeName, parameters);
    }

    private delegate DelegateInfo Updater(
        DelegateInfo delegateInfo,
        CancellationToken cancellationToken
    );
}
