using System.Threading;
using Microsoft.CodeAnalysis;

namespace MinimalLambda.SourceGenerators;

internal class GeneratorContext
{
    internal WellKnownTypes.WellKnownTypes WellKnownTypes { get; }
    internal CancellationToken CancellationToken { get; }
    internal SemanticModel SemanticModel { get; }
    internal GeneratorSyntaxContext GeneratorAttributeSyntaxContext { get; }

    internal SyntaxNode Node { get; }

    internal GeneratorContext(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        GeneratorAttributeSyntaxContext = context;
        Node = context.Node;
        SemanticModel = GeneratorAttributeSyntaxContext.SemanticModel;
        CancellationToken = cancellationToken;
        WellKnownTypes = SourceGenerators.WellKnownTypes.WellKnownTypes.GetOrCreate(
            context.SemanticModel.Compilation
        );
    }
}

internal static class GeneratorContextExtensions
{
    extension(GeneratorContext context)
    {
        public void ThrowIfCancellationRequested() =>
            context.CancellationToken.ThrowIfCancellationRequested();
    }
}
