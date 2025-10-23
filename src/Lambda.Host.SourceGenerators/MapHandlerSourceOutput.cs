using System.Collections.Generic;
using System.Linq;
using Lambda.Host.SourceGenerators.Models;
using Microsoft.CodeAnalysis;

namespace Lambda.Host.SourceGenerators;

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

        // if no MapHandler calls were found, we will silently exit early.
        if (compilationInfo.MapHandlerInvocationInfos.Count == 0)
            return;

        var mapHandlerInvocationInfo = compilationInfo.MapHandlerInvocationInfos.First();
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
                        AssignmentStatement = param switch
                        {
                            // Request -> deserialize to type
                            { Attributes: var attrs }
                                when attrs.Any(a => a.Type == AttributeConstants.EventAttribute) =>
                                $"context.GetEventT<{param.Type}>()",

                            // ILambdaContext OR ILambdaHostContext -> use context directly
                            {
                                Type: TypeConstants.ILambdaContext
                                    or TypeConstants.ILambdaHostContext
                            } => "context",

                            // CancellationToken -> get from context
                            { Type: TypeConstants.CancellationToken } =>
                                "context.CancellationToken",

                            // inject keyed service from the DI container
                            { Attributes: var attrs }
                                when attrs.FirstOrDefault(a =>
                                    a.Type == AttributeConstants.FromKeyedService
                                )
                                    is { Arguments: { Count: > 0 } args } =>
                                $"context.ServiceProvider.GetRequiredKeyedService<{param.Type}>(\"{args.First()}\")",

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

        var inputEvent = delegateInfo
            .Parameters.Where(p =>
                p.Attributes.Any(a => a.Type == AttributeConstants.EventAttribute)
            )
            .Select(p => new { IsStream = p.Type == TypeConstants.Stream, Type = p.Type })
            .FirstOrDefault();

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

        var template = TemplateHelper.LoadTemplate(GeneratorConstants.LambdaHandlerTemplateFile);

        var outCode = template.Render(model);

        context.AddSource("LambdaHandler.g.cs", outCode);
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
                invocationInfo.DelegateInfo.Parameters.Count(p =>
                    p.Attributes.Any(a => a.Type == AttributeConstants.EventAttribute)
                ) > 1
            )
                diagnostics.AddRange(
                    invocationInfo
                        .DelegateInfo.Parameters.Where(p =>
                            p.Attributes.Any(a => a.Type == AttributeConstants.EventAttribute)
                        )
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
