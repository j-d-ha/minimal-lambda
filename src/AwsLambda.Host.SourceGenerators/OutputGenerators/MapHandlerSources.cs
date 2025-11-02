using System.Linq;
using System.Text;
using AwsLambda.Host.SourceGenerators.Models;

namespace AwsLambda.Host.SourceGenerators;

internal static class MapHandlerSources
{
    internal static string Generate(HigherOrderMethodInfo higherOrderMethodInfo)
    {
        var delegateInfo = higherOrderMethodInfo.DelegateInfo;

        // build handler function signature
        var handlerSignature = delegateInfo.BuildHandlerSignature();

        // build out assignment statements for each handler parameter
        var handlerArgs = delegateInfo.BuildHandlerParameterAssignment();

        // get input event type
        var inputEvent = delegateInfo.EventParameter is { } p
            ? new { IsStream = p.Type == TypeConstants.Stream, p.Type }
            : null;

        // get output response type and whether it is a stream
        var outputResponse = delegateInfo.HasResponse
            ? new
            {
                ResponseType = delegateInfo.UnwrappedResponseType,
                ResponseIsStream = delegateInfo.UnwrappedResponseType == TypeConstants.Stream,
            }
            : null;

        var model = new
        {
            Location = higherOrderMethodInfo.InterceptableLocationInfo,
            HandlerSignature = handlerSignature,
            delegateInfo.HasAnyKeyedServiceParameter,
            HandlerArgs = handlerArgs,
            ShouldAwait = delegateInfo.IsAwaitable,
            InputEvent = inputEvent,
            OutputResponse = outputResponse,
        };

        var template = TemplateHelper.LoadTemplate(
            GeneratorConstants.LambdaHostMapHandlerExtensionsTemplateFile
        );

        return template.Render(model);
    }

    private static string BuildHandlerSignature(this DelegateInfo delegateInfo)
    {
        // build handler function signature
        var signatureBuilder = new StringBuilder();
        signatureBuilder.Append(delegateInfo.DelegateType);

        // angle brackets needed for any parameters or a response type
        if (
            delegateInfo.Parameters.Count > 0
            || delegateInfo.FullResponseType != TypeConstants.Void
        )
        {
            signatureBuilder.Append("<");

            // join parameters with comma
            if (delegateInfo.Parameters.Count > 0)
                signatureBuilder.Append(
                    string.Join(", ", delegateInfo.Parameters.Select(p => p.Type))
                );

            if (delegateInfo.FullResponseType != TypeConstants.Void)
            {
                // add comma if there are parameters, i.e. this is the last in the list
                if (delegateInfo.Parameters.Count > 0)
                    signatureBuilder.Append(", ");
                signatureBuilder.Append(delegateInfo.FullResponseType);
            }

            signatureBuilder.Append(">");
        }

        var handlerSignature = signatureBuilder.ToString();

        return handlerSignature;
    }

    private static HandlerArg[] BuildHandlerParameterAssignment(this DelegateInfo delegateInfo)
    {
        var handlerArgs = delegateInfo
            .Parameters.Select(param => new HandlerArg
            {
                String = param.ToPublicString(),
                Assignment = param.Source switch
                {
                    // Event -> deserialize to type
                    ParameterSource.Event => $"context.GetEventT<{param.Type}>()",

                    // ILambdaContext OR ILambdaHostContext -> use context directly
                    ParameterSource.Context => "context",

                    // CancellationToken -> get from context
                    ParameterSource.CancellationToken => "context.CancellationToken",

                    // inject keyed service from the DI container - required
                    ParameterSource.KeyedService when param.IsRequired =>
                        $"context.ServiceProvider.GetRequiredKeyedService<{param.Type}>(\"{param.KeyedServiceKey}\")",

                    // inject keyed service from the DI container - optional
                    ParameterSource.KeyedService =>
                        $"context.ServiceProvider.GetKeyedService<{param.Type}>(\"{param.KeyedServiceKey}\")",

                    // default: inject service from the DI container - required
                    _ when param.IsRequired =>
                        $"context.ServiceProvider.GetRequiredService<{param.Type}>()",

                    // default: inject service from the DI container - optional
                    _ => $"context.ServiceProvider.GetService<{param.Type}>()",
                },
            })
            .ToArray();

        return handlerArgs;
    }

    private readonly record struct HandlerArg(string String, string Assignment);
}
