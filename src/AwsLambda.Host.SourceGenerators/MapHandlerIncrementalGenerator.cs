using AwsLambda.Host.SourceGenerators.Models;
using AwsLambda.Host.SourceGenerators.Types;
using Microsoft.CodeAnalysis;

namespace AwsLambda.Host.SourceGenerators;

[Generator]
public class MapHandlerIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all MapHandler method calls with lambda analysis
        var mapHandlerCalls = context
            .SyntaxProvider.CreateSyntaxProvider(
                MapHandlerSyntaxProvider.Predicate,
                MapHandlerSyntaxProvider.Transformer
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!.Value);

        // find any calls to `UseOpenTelemetryTracing` and extract the location
        var openTelemetryTracingCalls = context
            .SyntaxProvider.CreateSyntaxProvider(
                UseOpenTelemetryTracingSyntaxProvider.Predicate,
                UseOpenTelemetryTracingSyntaxProvider.Transformer
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!.Value);

        // combine the compilation and map handler calls
        var combined = mapHandlerCalls
            .Collect()
            .Combine(openTelemetryTracingCalls.Collect())
            .Select(
                (t, _) => new CompilationInfo(t.Left.ToEquatableArray(), t.Right.ToEquatableArray())
            );

        // Generate source when calls are found
        context.RegisterSourceOutput(combined, LambdaHostOutputGenerator.Generate);
    }
}
