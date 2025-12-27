using System.Threading;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators;

internal static class MapHandlerSyntaxProvider
{
    internal static bool Predicate(SyntaxNode node, CancellationToken cancellationToken) =>
        HandlerInfoExtractor.Predicate(node, GeneratorConstants.MapHandlerMethodName);

    internal static InvocationMethodInfo? Transformer(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken
    ) => HandlerInfoExtractor.Transformer(context, _ => false, cancellationToken);
}
