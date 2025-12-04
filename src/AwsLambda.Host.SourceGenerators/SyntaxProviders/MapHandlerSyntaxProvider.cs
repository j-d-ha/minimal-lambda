using System.Threading;
using AwsLambda.Host.SourceGenerators.Models;
using Microsoft.CodeAnalysis;

namespace AwsLambda.Host.SourceGenerators;

internal static class MapHandlerSyntaxProvider
{
    internal static bool Predicate(SyntaxNode node, CancellationToken cancellationToken) =>
        HandlerInfoExtractor.Predicate(node, GeneratorConstants.MapHandlerMethodName);

    internal static HigherOrderMethodInfo? Transformer(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken
    ) => HandlerInfoExtractor.Transformer(context, _ => false, cancellationToken);
}
