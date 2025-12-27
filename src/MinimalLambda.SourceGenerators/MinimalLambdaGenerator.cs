using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators;

[Generator]
public class MinimalLambdaGenerator : IIncrementalGenerator
{
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

        // // Find all MapHandler method calls with lambda analysis
        // var mapHandlerCalls = context
        //     .SyntaxProvider.CreateSyntaxProvider(
        //         MapHandlerSyntaxProvider.Predicate,
        //         MapHandlerSyntaxProvider.Transformer
        //     )
        //     .Where(static m => m is not null)
        //     .Select(static (m, _) => m!.Value);
        //
        // // Find all OnShutdown method calls with lambda analysis
        // var onShutdownCalls = context
        //     .SyntaxProvider.CreateSyntaxProvider(
        //         OnShutdownSyntaxProvider.Predicate,
        //         OnShutdownSyntaxProvider.Transformer
        //     )
        //     .Where(static m => m is not null)
        //     .Select(static (m, _) => m!.Value);
        //
        // // Find all OnInit method calls with lambda analysis
        // var onInitCalls = context
        //     .SyntaxProvider.CreateSyntaxProvider(
        //         OnInitSyntaxProvider.Predicate,
        //         OnInitSyntaxProvider.Transformer
        //     )
        //     .Where(static m => m is not null)
        //     .Select(static (m, _) => m!.Value);
        //
        // // find LambdaApplicationBuilder.Build() calls
        // var lambdaApplicationBuilderBuildCalls = context
        //     .SyntaxProvider.CreateSyntaxProvider(
        //         LambdaApplicationBuilderBuildSyntaxProvider.Predicate,
        //         LambdaApplicationBuilderBuildSyntaxProvider.Transformer
        //     )
        //     .Where(static m => m is not null)
        //     .Select(static (m, _) => m!.Value);

        // handler registration calls
        var registrationCalls = context
            .SyntaxProvider.CreateSyntaxProvider(
                HandlerSyntaxProvider.Predicate,
                HandlerSyntaxProvider.Transformer
            )
            .WhereNotNull();

        // find UseMiddleware<T>() calls
        var useMiddlewareTCalls = context
            .SyntaxProvider.CreateSyntaxProvider(
                UseMiddlewareTSyntaxProvider.Predicate,
                UseMiddlewareTSyntaxProvider.Transformer
            )
            .WhereNotNull();

        // collect call
        // var mapHandlerCallsCollected = mapHandlerCalls.Collect();
        // var onShutdownCallsCollected = onShutdownCalls.Collect();
        // var onInitCallsCollected = onInitCalls.Collect();
        // var lambdaApplicationBuilderBuildCallsCollected =
        //     lambdaApplicationBuilderBuildCalls.Collect();

        var registrationCallsCollected = registrationCalls.Collect();
        var useMiddlewareTCallsCollected = useMiddlewareTCalls.Collect();

        // combine the compilation and map handler calls
        var combined = registrationCallsCollected
            .Combine(useMiddlewareTCallsCollected)
            .Select(
                CompilationInfo? (t, _) =>
                {
                    var (handlerInfos, useMiddlewareInfo) = t;

                    if (handlerInfos.Length == 0 && useMiddlewareInfo.Length == 0)
                        return null;

                    return new CompilationInfo
                    {
                        MapHandlerInvocationInfos = handlerInfos
                            .Where(h => h.MethodType == MethodType.MapHandler)
                            .ToEquatableArray(),
                        OnShutdownInvocationInfos = handlerInfos
                            .Where(h => h.MethodType == MethodType.OnShutdown)
                            .ToEquatableArray(),
                        OnInitInvocationInfos = handlerInfos
                            .Where(h => h.MethodType == MethodType.OnInit)
                            .ToEquatableArray(),
                        UseMiddlewareTInfos = useMiddlewareInfo.ToEquatableArray(),
                    };
                }
            );

        // Generate source when calls are found
        context.RegisterSourceOutput(
            combined,
            (productionContext, info) =>
            {
                if (info is null)
                    return;

                MinimalLambdaEmitter.Generate(productionContext, info.Value);
            }
        );
    }
}
