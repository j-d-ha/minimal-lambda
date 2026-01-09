using LayeredCraft.SourceGeneratorTools.Types;

namespace MinimalLambda.SourceGenerators.Models;

internal sealed record CompilationInfo(
    EquatableArray<MapHandlerMethodInfo> MapHandlerInvocationInfos,
    EquatableArray<LifecycleMethodInfo> OnShutdownInvocationInfos,
    EquatableArray<LifecycleMethodInfo> OnInitInvocationInfos,
    EquatableArray<UseMiddlewareTInfo> UseMiddlewareTInfos
);
