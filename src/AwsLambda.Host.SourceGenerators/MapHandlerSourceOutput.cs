using System.Collections.Generic;
using System.Linq;
using AwsLambda.Host.SourceGenerators.Models;
using AwsLambda.Host.SourceGenerators.Types;
using Microsoft.CodeAnalysis;

namespace AwsLambda.Host.SourceGenerators;

internal static class MapHandlerSourceOutput
{
    internal static void Generate(SourceProductionContext context, CompilationInfo compilationInfo)
    {
        // validate the generator data and report any diagnostics before exiting.
        var diagnostics = ValidateGeneratorData(compilationInfo);
        if (diagnostics.Any())
        {
            diagnostics.ForEach(context.ReportDiagnostic);
            return;
        }

        List<string?> outputs = [GenerateInterceptsLocationAttribute()];

        // if MapHandler calls found, generate the source code. Will always be 0 or 1 at this point.
        // Anything that needs to know types from the handler must be generated here.
        if (compilationInfo.MapHandlerInvocationInfos.Count == 1)
        {
            var mapHandlerInvocationInfo = compilationInfo.MapHandlerInvocationInfos.First();

            outputs.Add(GenerateLambdaHostMapHandlerExtensions(mapHandlerInvocationInfo));

            // if UseOpenTelemetryTracing calls found, generate the source code.
            if (compilationInfo.UseOpenTelemetryTracingInfos.Count >= 1)
                outputs.Add(
                    GenerateLambdaHostUseOpenTelemetryTracingExtensions(
                        compilationInfo.UseOpenTelemetryTracingInfos,
                        mapHandlerInvocationInfo.DelegateInfo
                    )
                );
        }

        var outCode = string.Join("\n", outputs.Where(s => s != null));

        context.AddSource("LambdaHandler.g.cs", outCode);
    }

    private static string? GenerateInterceptsLocationAttribute()
    {
        var template = TemplateHelper.LoadTemplate(
            GeneratorConstants.InterceptsLocationAttributeTemplateFile
        );

        return template.Render();
    }

    private static string? GenerateLambdaHostMapHandlerExtensions(
        MapHandlerInvocationInfo mapHandlerInvocationInfo
    )
    {
        var delegateInfo = mapHandlerInvocationInfo.DelegateInfo;

        var delegateArguments = delegateInfo
            .Parameters.Select(p => p.Type)
            .Concat(
                new[] { delegateInfo.ResponseType }.Where(t => t != null && t != TypeConstants.Void)
            )
            .ToList();

        var handlerArgs = delegateInfo
            .Parameters.Select(
                (param, index) =>
                    new
                    {
                        VarName = $"arg{index}",
                        AssignmentStatement = param.Source switch
                        {
                            // Event -> deserialize to type
                            ParameterSource.Event => $"context.GetEventT<{param.Type}>()",

                            // ILambdaContext OR ILambdaHostContext -> use context directly
                            ParameterSource.Context => "context",

                            // CancellationToken -> get from context
                            ParameterSource.ContextCancellation => "context.CancellationToken",

                            // inject keyed service from the DI container
                            ParameterSource.KeyedService =>
                                $"context.ServiceProvider.GetRequiredKeyedService<{param.Type}>(\"{param.KeyedServiceKey}\")",

                            // default: inject service from the DI container
                            _ => $"context.ServiceProvider.GetRequiredService<{param.Type}>()",
                        },
                    }
            )
            .ToArray();

        var shouldAwait = delegateInfo.ResponseType.StartsWith(TypeConstants.Task);

        // Unwrap Task<T>
        var responseType = delegateInfo.ResponseType;
        if (responseType.StartsWith(TypeConstants.Task + "<"))
        {
            var startIndex = responseType.IndexOf('<') + 1;
            var endIndex = responseType.LastIndexOf('>');
            responseType = responseType.Substring(startIndex, endIndex - startIndex);
        }

        var inputEvent = delegateInfo.EventParameter is { } p
            ? new { IsStream = p.Type == TypeConstants.Stream, Type = p.Type }
            : null;

        // 1. if Action -> no return
        // 3. if Func + Task return type + async -> no return
        // 2. if Func + Task return type -> return value
        // 4. if Func + non-Task return type -> return value
        var hasReturnValue = delegateInfo switch
        {
            { DelegateType: TypeConstants.Action } => false,
            { DelegateType: TypeConstants.Func, ResponseType: TypeConstants.Task } => false,
            _ => true,
        };

        var outputResponse = hasReturnValue
            ? new
            {
                ResponseIsStream = delegateInfo.ResponseType == TypeConstants.Stream,
                ResponseType = responseType,
                ResponseTypeIsNullable = responseType.EndsWith("?"),
            }
            : null;

        var model = new
        {
            Location = mapHandlerInvocationInfo.InterceptableLocationInfo,
            delegateInfo.DelegateType,
            DelegateArgs = delegateArguments,
            HandlerArgs = handlerArgs,
            ShouldAwait = shouldAwait,
            InputEvent = inputEvent,
            OutputResponse = outputResponse,
        };

        var template = TemplateHelper.LoadTemplate(
            GeneratorConstants.LambdaHostMapHandlerExtensionsTemplateFile
        );

        return template.Render(model);
    }

    private static string? GenerateLambdaHostUseOpenTelemetryTracingExtensions(
        EquatableArray<UseOpenTelemetryTracingInfo> useOpenTelemetryTracingInfos,
        DelegateInfo delegateInfo
    )
    {
        // get the handler input event type
        var eventType = delegateInfo.EventParameter is { } p ? p.Type : null;

        // get the handler output return type
        var responseType = delegateInfo.HasResponse ? delegateInfo.ResponseType : null;

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

    private static List<Diagnostic> ValidateGeneratorData(CompilationInfo compilationInfo)
    {
        var diagnostics = new List<Diagnostic>();

        var delegateInfos = compilationInfo.MapHandlerInvocationInfos;

        // check for multiple invocations of MapHandler
        if (delegateInfos.Count > 1)
            diagnostics.AddRange(
                delegateInfos.Select(invocationInfo =>
                    Diagnostic.Create(
                        Diagnostics.MultipleMethodCalls,
                        invocationInfo.LocationInfo?.ToLocation(),
                        "LambdaApplication.MapHandler(Delegate)"
                    )
                )
            );

        // Validate parameters
        foreach (var invocationInfo in delegateInfos)
        {
            // check for multiple parameters that use the `[Event]` attribute
            if (
                invocationInfo.DelegateInfo.Parameters.Count(p => p.Source == ParameterSource.Event)
                > 1
            )
                diagnostics.AddRange(
                    invocationInfo
                        .DelegateInfo.Parameters.Where(p => p.Source == ParameterSource.Event)
                        .Select(p =>
                            Diagnostic.Create(
                                Diagnostics.MultipleParametersUseAttribute,
                                p.LocationInfo?.ToLocation(),
                                AttributeConstants.EventAttribute
                            )
                        )
                );
        }

        return diagnostics;
    }
}
