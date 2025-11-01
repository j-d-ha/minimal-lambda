using System.Collections.Immutable;
using System.Linq;
using System.Text;
using AwsLambda.Host.SourceGenerators.Models;
using AwsLambda.Host.SourceGenerators.Types;

namespace AwsLambda.Host.SourceGenerators;

internal static class OnShutdownSources
{
    internal static string Generate(EquatableArray<HigherOrderMethodInfo> higherOrderMethodInfos)
    {
        var calls = higherOrderMethodInfos
            .Select(higherOrderMethodInfo =>
            {
                // get generic type arguments, either "" or "<T1, T2, ... >""
                var genericParameters =
                    higherOrderMethodInfo.GenericTypeArguments.Length > 0
                        ? $"<{string.Join(", ", higherOrderMethodInfo.GenericTypeArguments.Select(g => g.Parameter))}>"
                        : "";

                // build handler function signature
                var handlerSignature =
                    higherOrderMethodInfo.GenericTypeArguments.BuildHandlerSignature();

                // get arguments for handler
                var handlerArgs =
                    higherOrderMethodInfo.DelegateInfo.BuildHandlerParameterAssignment();

                return new
                {
                    Location = higherOrderMethodInfo.InterceptableLocationInfo,
                    GenericParameters = genericParameters,
                    HandlerSignature = handlerSignature,
                    HandlerArgs = handlerArgs,
                    HasHandlerArgs = handlerArgs.Length > 0,
                    higherOrderMethodInfo.DelegateInfo.HasAnyKeyedServiceParameter,
                };
            })
            .ToArray();

        var model = new { Calls = calls };

        var template = TemplateHelper.LoadTemplate(
            GeneratorConstants.LambdaHostOnShutdownExtensionsTemplateFile
        );

        return template.Render(model);
    }

    private static string BuildHandlerSignature(
        this ImmutableArray<GenericInfo> genericTypeArguments
    )
    {
        var signatureBuilder = new StringBuilder();
        signatureBuilder.Append("Func<");

        if (genericTypeArguments.Length > 0)
        {
            signatureBuilder.Append(
                string.Join(", ", genericTypeArguments.Select(g => g.Parameter))
            );
            signatureBuilder.Append(", ");
        }

        signatureBuilder.Append("Task>");

        var handlerSignature = signatureBuilder.ToString();

        return handlerSignature;
    }

    private static string[] BuildHandlerParameterAssignment(this DelegateInfo delegateInfo)
    {
        var handlerArgs = delegateInfo
            .Parameters.Select(
                (param, i) =>
                {
                    i++;
                    return param.Source switch
                    {
                        // CancellationToken -> get directly from arguments
                        ParameterSource.CancellationToken =>
                            $"Unsafe.As<CancellationToken, T{i}>(ref cancellationToken)",

                        // inject keyed service from the DI container - required
                        ParameterSource.KeyedService when param.IsRequired =>
                            $"serviceProvider.GetRequiredKeyedService<T{i}>(\"{param.KeyedServiceKey}\")",

                        // inject keyed service from the DI container - optional
                        ParameterSource.KeyedService =>
                            $"serviceProvider.GetKeyedService<T{i}>(\"{param.KeyedServiceKey}\")",

                        // default: inject service from the DI container - required
                        _ when param.IsRequired => $"serviceProvider.GetRequiredService<T{i}>()",

                        // default: inject service from the DI container - optional
                        _ => $"serviceProvider.GetService<T{i}>()",
                    };
                }
            )
            .ToArray();

        return handlerArgs;
    }
}
