using System.Collections.Immutable;

namespace Lambda.Host.SourceGenerators.Models;

internal readonly record struct CompilationInfo
{
    internal required ImmutableArray<MapHandlerInvocationInfo> MapHandlerInvocationInfos { get; init; }

    internal required ImmutableArray<StartupClassInfo?> StartupClassInfos { get; init; }
}
