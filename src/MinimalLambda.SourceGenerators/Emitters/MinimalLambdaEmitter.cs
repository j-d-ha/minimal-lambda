using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators;

internal static class MinimalLambdaEmitter
{
    internal static string GeneratedCodeAttribute
    {
        get
        {
            if (field is null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var generatorName = assembly.GetName().Name;
                var generatorVersion = assembly.GetName().Version.ToString();

                field =
                    $"""[global::System.CodeDom.Compiler.GeneratedCode("{generatorName}", "{generatorVersion}")]""";
            }

            return field;
        }
    }

    internal static void Generate(SourceProductionContext context, CompilationInfo compilationInfo)
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

        List<string?> outputs =
        [
            CommonSources.Generate(),
            """
                namespace MinimalLambda.Generated
                {
                    using System;
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
            outputs.Add(MapHandlerSources.Generate(compilationInfo.MapHandlerInvocationInfos));

        // add UseMiddleware<T> interceptors
        if (compilationInfo.UseMiddlewareTInfos.Count >= 1)
            outputs.Add(UseMiddlewareTSource.Generate(compilationInfo.UseMiddlewareTInfos));

        // add OnInit interceptors
        if (compilationInfo.OnInitInvocationInfos.Count >= 1)
            outputs.Add(
                GenericHandlerSources.Generate(
                    compilationInfo.OnInitInvocationInfos,
                    "OnInit",
                    "bool",
                    "true",
                    "ILambdaOnInitBuilder"
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
                    "ILambdaOnShutdownBuilder"
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
