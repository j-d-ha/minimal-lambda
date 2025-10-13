using System.Collections.Immutable;

namespace Lambda.Host.SourceGenerators.Models;

internal readonly record struct CompilationInfo(
    ImmutableArray<MapHandlerInvocationInfo> MapHandlerInvocationInfos,
    ImmutableArray<StartupClassInfo?> StartupClassInfos
);
