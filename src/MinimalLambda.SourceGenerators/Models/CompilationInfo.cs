using LayeredCraft.SourceGeneratorTools.Types;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct CompilationInfo(
    EquatableArray<HigherOrderMethodInfo> MapHandlerInvocationInfos,
    EquatableArray<HigherOrderMethodInfo> OnShutdownInvocationInfos,
    EquatableArray<HigherOrderMethodInfo> OnInitInvocationInfos,
    EquatableArray<SimpleMethodInfo> BuilderInfos,
    EquatableArray<UseMiddlewareTInfo> UseMiddlewareTInfos
);
