using System.Linq;
using System.Reflection;
using AwsLambda.Host.SourceGenerators.Models;
using AwsLambda.Host.SourceGenerators.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AwsLambda.Host.SourceGenerators;

[Generator]
public class MapHandlerIncrementalGenerator : IIncrementalGenerator
{
    private readonly string _generatorName;
    private readonly string _generatorVersion;

    public MapHandlerIncrementalGenerator()
    {
        // Get the embedded metadata attributes
        var metadataAttributes = typeof(MapHandlerIncrementalGenerator)
            .Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .ToList();

        _generatorName =
            metadataAttributes.FirstOrDefault(a => a.Key == "SourceGeneratorName")?.Value
            ?? "UNKNOWN";

        _generatorVersion =
            metadataAttributes.FirstOrDefault(a => a.Key == "SourceGeneratorVersion")?.Value
            ?? "UNKNOWN";
    }

    /// <summary>This constructor is only used for testing.</summary>
    internal MapHandlerIncrementalGenerator(string generatorName, string generatorVersion)
    {
        _generatorName = generatorName;
        _generatorVersion = generatorVersion;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Language version gate - only generate source if C# 11 or later is used
        var csharpSufficient = context.CompilationProvider.Select(
            static (compilation, _) =>
                compilation
                    is CSharpCompilation
                    {
                        LanguageVersion: LanguageVersion.Default or >= LanguageVersion.CSharp11,
                    }
        );

        context.RegisterSourceOutput(
            csharpSufficient,
            static (spc, ok) =>
            {
                if (!ok)
                    spc.ReportDiagnostic(
                        Diagnostic.Create(Diagnostics.CSharpVersionTooLow, Location.None)
                    );
            }
        );

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

        // Find all OnInit method calls with lambda analysis
        var onInitCalls = context
            .SyntaxProvider.CreateSyntaxProvider(
                OnInitSyntaxProvider.Predicate,
                OnInitSyntaxProvider.Transformer
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

        // find LambdaApplicationBuilder.Build() calls
        var lambdaApplicationBuilderBuildCalls = context
            .SyntaxProvider.CreateSyntaxProvider(
                LambdaApplicationBuilderBuildSyntaxProvider.Predicate,
                LambdaApplicationBuilderBuildSyntaxProvider.Transformer
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!.Value);

        // collect call
        var mapHandlerCallsCollected = mapHandlerCalls.Collect();
        var onShutdownCallsCollected = onShutdownCalls.Collect();
        var onInitCallsCollected = onInitCalls.Collect();
        var openTelemetryTracingCallsCollected = openTelemetryTracingCalls.Collect();
        var lambdaApplicationBuilderBuildCallsCollected =
            lambdaApplicationBuilderBuildCalls.Collect();

        // combine the compilation and map handler calls
        var combined = mapHandlerCallsCollected
            .Combine(onShutdownCallsCollected)
            .Combine(onInitCallsCollected)
            .Combine(openTelemetryTracingCallsCollected)
            .Combine(lambdaApplicationBuilderBuildCallsCollected)
            .Select(
                CompilationInfo? (t, _) =>
                {
                    if (
                        t.Left.Left.Left.Left.Length == 0
                        && t.Left.Left.Left.Right.Length == 0
                        && t.Left.Left.Right.Length == 0
                        && t.Left.Right.Length == 0
                        && t.Right.Length == 0
                    )
                        return null;

                    return new CompilationInfo(
                        t.Left.Left.Left.Left.ToEquatableArray(),
                        t.Left.Left.Left.Right.ToEquatableArray(),
                        t.Left.Left.Right.ToEquatableArray(),
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

                LambdaHostOutputGenerator.Generate(
                    productionContext,
                    info.Value,
                    _generatorName,
                    _generatorVersion
                );
            }
        );
    }
}
