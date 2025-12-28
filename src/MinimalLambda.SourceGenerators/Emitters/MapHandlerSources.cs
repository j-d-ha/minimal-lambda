using LayeredCraft.SourceGeneratorTools.Types;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators;

internal static class MapHandlerSources
{
    internal static string Generate(
        EquatableArray<MapHandlerMethodInfo> mapHandlerInvocationInfos
    ) =>
        TemplateHelper.Render(
            GeneratorConstants.LambdaHostMapHandlerExtensionsTemplateFile,
            new
            {
                MinimalLambdaEmitter.GeneratedCodeAttribute,
                MapHandlerCalls = mapHandlerInvocationInfos,
            }
        );
}
