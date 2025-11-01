using System.Collections.Generic;
using System.Linq;
using AwsLambda.Host.SourceGenerators.Models;
using Microsoft.CodeAnalysis;

namespace AwsLambda.Host.SourceGenerators;

internal static class LambdaHostOutputGenerator
{
    internal static void Generate(SourceProductionContext context, CompilationInfo compilationInfo)
    {
        // validate the generator data and report any diagnostics before exiting.
        var diagnostics = DiagnosticGenerator.GenerateDiagnostics(compilationInfo);
        if (diagnostics.Any())
        {
            diagnostics.ForEach(context.ReportDiagnostic);
            return;
        }

        List<string?> outputs = [CommonSources.Generate()];

        // if MapHandler calls found, generate the source code. Will always be 0 or 1 at this point.
        // Anything that needs to know types from the handler must be generated here.
        if (compilationInfo.MapHandlerInvocationInfos.Count == 1)
        {
            var mapHandlerInvocationInfo = compilationInfo.MapHandlerInvocationInfos.First();

            outputs.Add(MapHandlerSources.Generate(mapHandlerInvocationInfo));

            // if UseOpenTelemetryTracing calls found, generate the source code.
            if (compilationInfo.UseOpenTelemetryTracingInfos.Count >= 1)
                outputs.Add(
                    OpenTelemetrySources.Generate(
                        compilationInfo.UseOpenTelemetryTracingInfos,
                        mapHandlerInvocationInfo.DelegateInfo
                    )
                );
        }

        // add OnShutdown interceptors
        if (compilationInfo.OnShutdownInvocationInfos.Count >= 1)
            outputs.Add(OnShutdownSources.Generate(compilationInfo.OnShutdownInvocationInfos));

        // join all the source code together and add it to the compilation context.
        var outCode = string.Join("\n", outputs.Where(s => s != null));

        // add the source code to the compilation context.
        context.AddSource("LambdaHandler.g.cs", outCode);
    }
}
