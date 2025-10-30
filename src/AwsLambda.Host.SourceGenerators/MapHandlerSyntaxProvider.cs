using System.Threading;
using AwsLambda.Host.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AwsLambda.Host.SourceGenerators;

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
                Name.Identifier.ValueText: GeneratorConstants.MapHandlerMethodName,
            };
    }

    /// <summary>
    ///     Extracts a <see cref="MapHandlerInvocationInfo" /> object from the given syntax context
    ///     if the syntax node represents a valid MapHandler invocation.
    /// </summary>
    /// <param name="context">The context containing the syntax information and semantic model.</param>
    /// <param name="cancellationToken">The cancellation token to observe cancellation requests.</param>
    /// <returns>
    ///     A <see cref="MapHandlerInvocationInfo" /> object containing details about the delegate if the
    ///     syntax
    ///     corresponds to a valid handler invocation; otherwise, <c>null</c>.
    /// </returns>
    internal static MapHandlerInvocationInfo? Transformer(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        // validate that the method is from the LambdaApplication type
        if (context.Node is not InvocationExpressionSyntax invocationExpr)
            return null;

        var symbolInfo = ModelExtensions.GetSymbolInfo(
            context.SemanticModel,
            invocationExpr,
            cancellationToken
        );

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return null;

        // Check if it's from LambdaApplication
        // if (methodSymbol.ContainingType?.Name != GeneratorConstants.StartupClassName)
        //     return null;

        // TODO: make sure it's the right overload

        var delegateInfo = invocationExpr.ExtractDelegateInfo(context, cancellationToken);
        if (delegateInfo is null)
            return null;

        // get interceptable location
        var interceptableLocation = context.SemanticModel.GetInterceptableLocation(
            invocationExpr,
            cancellationToken
        )!;

        return new MapHandlerInvocationInfo(
            LocationInfo: LocationInfo.CreateFrom(context.Node),
            DelegateInfo: delegateInfo.Value,
            InterceptableLocationInfo: InterceptableLocationInfo.CreateFrom(interceptableLocation)
        );
    }
}
