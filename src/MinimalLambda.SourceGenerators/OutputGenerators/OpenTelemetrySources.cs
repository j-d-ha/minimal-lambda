using System.Linq;
using LayeredCraft.SourceGeneratorTools.Types;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators;

internal static class OpenTelemetrySources
{
    internal static string Generate(
        EquatableArray<SimpleMethodInfo> useOpenTelemetryTracingInfos,
        DelegateInfo delegateInfo,
        string generatedCodeAttribute
    )
    {
        // get the handler input event type
        var eventType = delegateInfo.EventParameter is { } p ? p.TypeInfo.FullyQualifiedType : null;

        // get the handler output return type
        var responseType = delegateInfo.HasResponse
            ? delegateInfo.ReturnTypeInfo.UnwrappedFullyQualifiedType
            : null;

        // interceptable locations
        var locations = useOpenTelemetryTracingInfos.Select(u => u.InterceptableLocationInfo);

        var model = new
        {
            Locations = locations,
            EventType = eventType,
            ResponseType = responseType,
            GeneratedCodeAttribute = generatedCodeAttribute,
        };

        var template = TemplateHelper.LoadTemplate(
            GeneratorConstants.LambdaHostUseOpenTelemetryTracingExtensionsTemplateFile
        );

        return template.Render(model);
    }
}
