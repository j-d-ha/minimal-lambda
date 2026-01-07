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
                            .OfType<MapHandlerMethodInfo>()
                            .ToEquatableArray(),
                        OnShutdownInvocationInfos = handlerInfos
                            .OfType<LifecycleMethodInfo>()
                            .Where(h => h.MethodType == MethodType.OnShutdown)
                            .ToEquatableArray(),
                        OnInitInvocationInfos = handlerInfos
                            .OfType<LifecycleMethodInfo>()
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
