using Lambda.Host.SourceGenerators.Types;

namespace Lambda.Host.SourceGenerators.Models;

internal readonly record struct CompilationInfo(
    EquatableArray<MapHandlerInvocationInfo> MapHandlerInvocationInfos,
    EquatableArray<StartupClassInfo> StartupClassInfos
);
