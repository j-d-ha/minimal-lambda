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

        // Find all OnShutdown method calls with lambda analysis
        var onShutdownCalls = context
            .SyntaxProvider.CreateSyntaxProvider(
                OnShutdownSyntaxProvider.Predicate,
                OnShutdownSyntaxProvider.Transformer
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

        // collect call
        var mapHandlerCallsCollected = mapHandlerCalls.Collect();
        var onShutdownCallsCollected = onShutdownCalls.Collect();
        var openTelemetryTracingCallsCollected = openTelemetryTracingCalls.Collect();

        // combine the compilation and map handler calls
        var combined = mapHandlerCallsCollected
            .Combine(onShutdownCallsCollected)
            .Combine(openTelemetryTracingCallsCollected)
            .Select(
                CompilationInfo? (t, _) =>
                {
                    if (t.Left.Left.Length == 0 && t.Left.Right.Length == 0 && t.Right.Length == 0)
                        return null;

                    return new CompilationInfo(
                        t.Left.Left.ToEquatableArray(),
                        t.Left.Right.ToEquatableArray(),
                        t.Right.ToEquatableArray()
                    );
                }
            );

        // Generate source when calls are found
        context.RegisterSourceOutput(
            combined,
            (productionContext, info) =>
            {
                if (info is null)
                    return;

                LambdaHostOutputGenerator.Generate(productionContext, info.Value);
            }
        );
    }
}
