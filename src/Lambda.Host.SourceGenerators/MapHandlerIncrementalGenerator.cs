using System.Linq;
using Lambda.Host.SourceGenerators.Models;
using Microsoft.CodeAnalysis;

namespace Lambda.Host.SourceGenerators;

[Generator]
public class MapHandlerIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var startupClassInfo = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                AttributeConstants.LambdaHostAttribute,
                LambdaHostSyntaxProvider.Predicate,
                LambdaHostSyntaxProvider.Transformer
            )
            .Where(static m => m is not null);

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
                MapHandlerSyntaxProvider.Predicate,
                MapHandlerSyntaxProvider.Transformer
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        // combine the compilation and map handler calls
        var combined = mapHandlerCalls
            .Collect()
            .Combine(compilationHasErrors)
            .Combine(startupClassInfo.Collect())
            .Select(
                (t, _) =>
                    new CompilationInfo
                    {
                        MapHandlerInvocationInfos = t.Left.Left,
                        CompilationHasErrors = t.Left.Right,
                        StartupClassInfos = t.Right,
                    }
            );

        // Generate source when calls are found
        context.RegisterSourceOutput(combined, MapHandlerSourceOutput.Generate);
    }
}
