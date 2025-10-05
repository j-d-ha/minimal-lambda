using System.Threading;
using Lambda.Host.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lambda.Host.SourceGenerators;

/// <summary>
///     Provides reusable helper methods for analyzing MapHandler syntax.
///     These methods can be used by both incremental source generators and diagnostic analyzers.
/// </summary>
internal static class MapHandlerSyntaxHelper
{
    /// <summary>
    ///     Determines whether the specified <paramref name="node" /> represents a valid invocation of the
    ///     method identified by <see cref="GeneratorConstants.MapHandlerMethodName" />.
    /// </summary>
    /// <param name="node">The syntax node to evaluate.</param>
    /// <param name="cancellationToken">The cancellation token used to observe cancellation requests.</param>
    /// <returns>
    ///     <c>true</c> if the specified <paramref name="node" /> is an invocation of the MapHandler
    ///     method; otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsMapHandlerInvocation(
        SyntaxNode node,
        CancellationToken cancellationToken
    )
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
    ///     Attempts to extract <see cref="MapHandlerInvocationInfo" /> from the given invocation
    ///     expression.
    /// </summary>
    /// <param name="invocation">The invocation expression syntax to analyze.</param>
    /// <param name="semanticModel">The semantic model for symbol resolution.</param>
    /// <param name="cancellationToken">The cancellation token to observe cancellation requests.</param>
    /// <param name="invocationInfo">
    ///     When this method returns, contains the invocation info if the analysis was successful;
    ///     otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the invocation represents a valid MapHandler call and info was extracted;
    ///     otherwise, <c>false</c>.
    /// </returns>
    internal static bool TryGetMapHandlerInfo(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken,
        out MapHandlerInvocationInfo? invocationInfo
    )
    {
        invocationInfo = null;

        // Validate that the method is from the LambdaApplication type
        var symbolInfo = semanticModel.GetSymbolInfo(invocation);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return false;

        // Check if it's from LambdaApplication
        if (methodSymbol.ContainingType?.Name != GeneratorConstants.StartupClassName)
            return false;

        var result = MapHandlerSyntaxProvider.ExtractDelegateInfo(
            invocation,
            semanticModel,
            cancellationToken
        );

        if (result is null)
            return false;

        invocationInfo = new MapHandlerInvocationInfo
        {
            LocationInfo = LocationInfo.CreateFrom(invocation.GetLocation()),
            DelegateInfo = result,
        };

        return true;
    }
}
