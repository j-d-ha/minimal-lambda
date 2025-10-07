using System.Linq;
using Microsoft.CodeAnalysis;

namespace Lambda.Host.SourceGenerators;

[Generator]
public class MapHandlerIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get the compilation and check for errors
        var compilationHasErrors = context.CompilationProvider.Select(
            (compilation, cancellationToken) =>
                compilation
                    .GetDiagnostics(cancellationToken)
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Any(d =>
                        d.Location.SourceTree != null
                        && !d.Location.SourceTree.FilePath.Contains(".g.cs")
                    )
        );

        // Find all MapHandler method calls with lambda analysis
        var mapHandlerCalls = context
            .SyntaxProvider.CreateSyntaxProvider(
                static (node, token) => MapHandlerSyntaxProvider.Predicate(node, token),
                static (ctx, cancellationToken) =>
                    MapHandlerSyntaxProvider.Transformer(ctx, cancellationToken)
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        // combine the compilation and map handler calls
        var combined = mapHandlerCalls.Collect().Combine(compilationHasErrors);

        // Generate source when calls are found
        context.RegisterSourceOutput(combined, MapHandlerSourceOutput.Generate);
    }
}
