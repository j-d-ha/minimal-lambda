using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators.Emitters;

internal static class InvocationHandlerEmitter
{
    private const string LambdaHostMapHandlerExtensionsTemplateFile =
        "Templates/MapHandler.scriban";

    internal static void Emit(
        SourceProductionContext context,
        ImmutableArray<MapHandlerMethodInfo> infos
    )
    {
        if (infos.Length == 0)
            return;

        var code = TemplateHelper.Render(
            LambdaHostMapHandlerExtensionsTemplateFile,
            new { TemplateHelper.GeneratedCodeAttribute, MapHandlerCalls = infos }
        );

        context.AddSource("MinimalLambda.InvocationHandlers.g.cs", code);
    }
}
