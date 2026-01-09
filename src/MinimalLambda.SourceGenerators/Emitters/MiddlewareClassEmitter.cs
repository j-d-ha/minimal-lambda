using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators.Emitters;

internal static class MiddlewareClassEmitter
{
    private const string UseMiddlewareTTemplateFile = "Templates/UseMiddlewareT.scriban";

    internal static void Emit(
        SourceProductionContext context,
        ImmutableArray<UseMiddlewareTInfo> infos
    )
    {
        if (infos.Length == 0)
            return;

        var code = TemplateHelper.Render(
            UseMiddlewareTTemplateFile,
            new { TemplateHelper.GeneratedCodeAttribute, Calls = infos }
        );

        context.AddSource("MinimalLambda.UseMiddleware.g.cs", code);
    }
}
