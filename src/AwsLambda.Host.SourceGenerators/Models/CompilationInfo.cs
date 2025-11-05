using AwsLambda.Host.SourceGenerators.Types;

namespace AwsLambda.Host.SourceGenerators.Models;

internal readonly record struct CompilationInfo(
    EquatableArray<HigherOrderMethodInfo> MapHandlerInvocationInfos,
    EquatableArray<HigherOrderMethodInfo> OnShutdownInvocationInfos,
    EquatableArray<HigherOrderMethodInfo> OnInitInvocationInfos,
    EquatableArray<UseOpenTelemetryTracingInfo> UseOpenTelemetryTracingInfos
);
