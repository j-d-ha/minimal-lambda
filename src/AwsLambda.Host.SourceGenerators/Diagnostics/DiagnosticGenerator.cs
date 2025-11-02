using System.Collections.Generic;
using System.Linq;
using AwsLambda.Host.SourceGenerators.Models;
using AwsLambda.Host.SourceGenerators.Types;
using Microsoft.CodeAnalysis;

namespace AwsLambda.Host.SourceGenerators;

internal static class DiagnosticGenerator
{
    internal static List<Diagnostic> GenerateDiagnostics(CompilationInfo compilationInfo)
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

        // check for invalid keyed service usage - MapHandler
        diagnostics.AddRange(
            compilationInfo.MapHandlerInvocationInfos.GenerateKeyedServiceKeyDiagnostics()
        );

        // check for invalid keyed service usage - OnShutdown
        diagnostics.AddRange(
            compilationInfo.OnShutdownInvocationInfos.GenerateKeyedServiceKeyDiagnostics()
        );

        return diagnostics;
    }

    private static Diagnostic[] GenerateKeyedServiceKeyDiagnostics(
        this EquatableArray<HigherOrderMethodInfo> methodNameInfos
    ) =>
        methodNameInfos
            .SelectMany(onShutdownInvocationInfo =>
                onShutdownInvocationInfo.DelegateInfo.Parameters
            )
            .Where(parameterInfo => parameterInfo.KeyedServiceKey is { DisplayValue: null })
            .Select(parameterInfo =>
                Diagnostic.Create(
                    Diagnostics.InvalidAttributeArgument,
                    parameterInfo.KeyedServiceKey!.Value.LocationInfo?.ToLocation(),
                    parameterInfo.KeyedServiceKey.Value.Type
                )
            )
            .ToArray();
}
