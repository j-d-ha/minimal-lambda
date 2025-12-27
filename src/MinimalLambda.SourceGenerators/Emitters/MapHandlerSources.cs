using LayeredCraft.SourceGeneratorTools.Types;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators;

internal static class MapHandlerSources
{
    internal static string Generate(EquatableArray<InvocationMethodInfo> mapHandlerInvocationInfos)
    {
        // var mapHandlerCalls = mapHandlerInvocationInfos.Select(mapHandler =>
        // {
        //     return new
        //     {
        //         InterceptableLocationAttribute =
        // mapHandler.InterceptableLocationInfo.ToInterceptsLocationAttribute(),
        //         HandlerSignature = handlerSignature,
        //         IsEventFeatureRequired = isEventFeatureRequired,
        //         IsResponseFeatureRequired = isResponseFeatureRequired,
        //         delegateInfo.HasAnyKeyedServiceParameter,
        //         HandlerArgs = handlerArgs,
        //         ShouldAwait = delegateInfo.IsAwaitable,
        //         InputEvent = inputEvent,
        //         OutputResponse = outputResponse,
        //     };
        // });

        var template = TemplateHelper.LoadTemplate(
            GeneratorConstants.LambdaHostMapHandlerExtensionsTemplateFile
        );

        return template.Render(
            new
            {
                MinimalLambdaEmitter.GeneratedCodeAttribute,
                MapHandlerCalls = mapHandlerInvocationInfos,
            }
        );
    }
}
