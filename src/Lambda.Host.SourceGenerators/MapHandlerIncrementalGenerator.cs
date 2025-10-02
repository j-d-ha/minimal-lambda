using Microsoft.CodeAnalysis;

namespace Lambda.Host.SourceGenerators;

[Generator]
public class MapHandlerIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all MapHandler method calls with lambda analysis
        var mapHandlerCalls = context
            .SyntaxProvider.CreateSyntaxProvider(
                static (node, token) => MapHandlerSyntaxProvider.Predicate(node, token),
                static (ctx, cancellationToken) =>
                    MapHandlerSyntaxProvider.Transformer(ctx, cancellationToken)
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        // Generate source when calls are found
        context.RegisterSourceOutput(
            mapHandlerCalls.Collect(),
            static (spc, calls) => MapHandlerSourceOutput.Generate(spc, calls)
        );
    }
}
