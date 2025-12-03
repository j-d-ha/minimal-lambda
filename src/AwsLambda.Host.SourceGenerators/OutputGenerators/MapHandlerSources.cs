#region

using System.Linq;
using AwsLambda.Host.SourceGenerators.Extensions;
using AwsLambda.Host.SourceGenerators.Models;
using AwsLambda.Host.SourceGenerators.Types;

#endregion

namespace AwsLambda.Host.SourceGenerators;

internal static class MapHandlerSources
{
    internal static string Generate(
        EquatableArray<HigherOrderMethodInfo> mapHandlerInvocationInfos,
        EquatableArray<SimpleMethodInfo> builderInfo,
        string generatedCodeAttribute
    )
    {
        var mapHandlerCalls = mapHandlerInvocationInfos.Select(mapHandlerInvocationInfo =>
        {
            var delegateInfo = mapHandlerInvocationInfo.DelegateInfo;

            // build handler function signature
            var handlerSignature = delegateInfo.BuildHandlerSignature();

            // build out assignment statements for each handler parameter
            var handlerArgs = delegateInfo.BuildHandlerParameterAssignment();

            // get input event type
            var inputEvent = delegateInfo.EventParameter is { } p
                ? new
                {
                    IsStream = p.TypeInfo.FullyQualifiedType == TypeConstants.Stream,
                    Type = p.TypeInfo.FullyQualifiedType,
                }
                : null;

            // get output response type and whether it is a stream
            var outputResponse = delegateInfo.HasResponse
                ? new
                {
                    ResponseType = delegateInfo.ReturnTypeInfo.UnwrappedFullyQualifiedType,
                    ResponseIsStream = delegateInfo.ReturnTypeInfo.UnwrappedFullyQualifiedType
                        == TypeConstants.Stream,
                }
                : null;

            // determine if event feature is required
            var isEventFeatureRequired = inputEvent is { IsStream: false };

            // determine if response feature is required
            var isResponseFeatureRequired = outputResponse is { ResponseIsStream: false };

            return new
            {
                Location = mapHandlerInvocationInfo.InterceptableLocationInfo,
                HandlerSignature = handlerSignature,
                IsEventFeatureRequired = isEventFeatureRequired,
                IsResponseFeatureRequired = isResponseFeatureRequired,
                delegateInfo.HasAnyKeyedServiceParameter,
                HandlerArgs = handlerArgs,
                ShouldAwait = delegateInfo.IsAwaitable,
                InputEvent = inputEvent,
                OutputResponse = outputResponse,
            };
        });

        var template = TemplateHelper.LoadTemplate(
            GeneratorConstants.LambdaHostMapHandlerExtensionsTemplateFile
        );

        return template.Render(
            new
            {
                GeneratedCodeAttribute = generatedCodeAttribute,
                MapHandlerCalls = mapHandlerCalls,
            }
        );
    }

    private static HandlerArg[] BuildHandlerParameterAssignment(this DelegateInfo delegateInfo) =>
        delegateInfo
            .Parameters.Select(param => new HandlerArg
            {
                String = param.ToPublicString(),
                Assignment = param.Source switch
                {
                    // Event -> deserialize to type
                    ParameterSource.Event
                        when param.TypeInfo.FullyQualifiedType == TypeConstants.Stream =>
                        "context.Features.GetRequired<IInvocationDataFeature>().EventStream",

                    ParameterSource.Event =>
                        $"context.GetRequiredEvent<{param.TypeInfo.FullyQualifiedType}>()",

                    // ILambdaContext OR ILambdaHostContext -> use context directly
                    ParameterSource.Context => "context",

                    // CancellationToken -> get from context
                    ParameterSource.CancellationToken => "context.CancellationToken",

                    // inject keyed service from the DI container - required
                    ParameterSource.KeyedService when param.IsRequired =>
                        $"context.ServiceProvider.GetRequiredKeyedService<{param.TypeInfo.FullyQualifiedType}>({param.KeyedServiceKey?.DisplayValue})",

                    // inject keyed service from the DI container - optional
                    ParameterSource.KeyedService =>
                        $"context.ServiceProvider.GetKeyedService<{param.TypeInfo.FullyQualifiedType}>({param.KeyedServiceKey?.DisplayValue})",

                    // default: inject service from the DI container - required
                    _ when param.IsRequired =>
                        $"context.ServiceProvider.GetRequiredService<{param.TypeInfo.FullyQualifiedType}>()",

                    // default: inject service from the DI container - optional
                    _ =>
                        $"context.ServiceProvider.GetService<{param.TypeInfo.FullyQualifiedType}>()",
                },
            })
            .ToArray();

    private readonly record struct HandlerArg(string String, string Assignment);
}
