using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators;

internal static class LambdaHostOutputGenerator
{
    internal static void Generate(
        SourceProductionContext context,
        CompilationInfo compilationInfo,
        string generatorName,
        string generatorVersion
    )
    {
        // validate the generator data and report any diagnostics before exiting.
        var diagnostics = DiagnosticGenerator.GenerateDiagnostics(compilationInfo);
        if (diagnostics.Any())
        {
            diagnostics.ForEach(context.ReportDiagnostic);

            // if there are any errors, return without generating any source code.
            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                return;
        }

        // create GeneratedCodeAttribute. This is used across all generated source files.
        var generatedCodeAttribute =
            $"[GeneratedCode(\"{generatorName}\", \"{generatorVersion}\")]";

        List<string?> outputs =
        [
            CommonSources.Generate(generatedCodeAttribute),
            """
                namespace MinimalLambda.Generated
                {
                    using System;
                    using System.CodeDom.Compiler;
                    using System.Runtime.CompilerServices;
                    using System.Threading;
                    using System.Threading.Tasks;
                    using Microsoft.Extensions.DependencyInjection;
                    using MinimalLambda;
                    using MinimalLambda.Builder;

                """,
        ];

        // if MapHandler calls found, generate the source code.
        if (compilationInfo.MapHandlerInvocationInfos.Count >= 1)
            outputs.Add(
                MapHandlerSources.Generate(
                    compilationInfo.MapHandlerInvocationInfos,
                    compilationInfo.BuilderInfos,
                    generatedCodeAttribute
                )
            );

        // add OnInit interceptors
        if (compilationInfo.OnInitInvocationInfos.Count >= 1)
            outputs.Add(
                GenericHandlerSources.Generate(
                    compilationInfo.OnInitInvocationInfos,
                    "OnInit",
                    "bool",
                    "true",
                    "ILambdaOnInitBuilder",
                    generatedCodeAttribute
                )
            );

        // add OnShutdown interceptors
        if (compilationInfo.OnShutdownInvocationInfos.Count >= 1)
            outputs.Add(
                GenericHandlerSources.Generate(
                    compilationInfo.OnShutdownInvocationInfos,
                    "OnShutdown",
                    null,
                    null,
                    "ILambdaOnShutdownBuilder",
                    generatedCodeAttribute
                )
            );

        outputs.Add(
            """
                file static class Utilities
                {
                    internal static T Cast<T>(Delegate d, T _) where T : Delegate => (T)d;
                }
            }
            """
        );

        // join all the source code together and add it to the compilation context.
        var outCode = string.Join("\n", outputs.Where(s => s != null));

        // add the source code to the compilation context.
        context.AddSource("LambdaHandler.g.cs", outCode);
    }
}
