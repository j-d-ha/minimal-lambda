using System.Linq;
using AwsLambda.Host.SourceGenerators.Models;
using AwsLambda.Host.SourceGenerators.Types;

namespace AwsLambda.Host.SourceGenerators;

internal static class OpenTelemetrySources
{
    internal static string Generate(
        EquatableArray<UseOpenTelemetryTracingInfo> useOpenTelemetryTracingInfos,
        DelegateInfo delegateInfo
    )
    {
        // get the handler input event type
        var eventType = delegateInfo.EventParameter is { } p ? p.Type : null;

        // get the handler output return type
        var responseType = delegateInfo.HasResponse ? delegateInfo.UnwrappedResponseType : null;

        // interceptable locations
        var locations = useOpenTelemetryTracingInfos.Select(u => u.InterceptableLocationInfo);

        var model = new
        {
            Locations = locations,
            EventType = eventType,
            ResponseType = responseType,
        };

        var template = TemplateHelper.LoadTemplate(
            GeneratorConstants.LambdaHostUseOpenTelemetryTracingExtensionsTemplateFile
        );

        return template.Render(model);
    }
}
