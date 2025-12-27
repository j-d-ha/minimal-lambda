using LayeredCraft.SourceGeneratorTools.Types;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct CompilationInfo(
    EquatableArray<InvocationMethodInfo> MapHandlerInvocationInfos,
    EquatableArray<LifecycleMethodInfo> OnShutdownInvocationInfos,
    EquatableArray<LifecycleMethodInfo> OnInitInvocationInfos,
    EquatableArray<UseMiddlewareTInfo> UseMiddlewareTInfos
);
