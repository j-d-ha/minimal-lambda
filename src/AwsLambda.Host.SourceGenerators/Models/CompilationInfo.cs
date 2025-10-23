using AwsLambda.Host.SourceGenerators.Types;

namespace AwsLambda.Host.SourceGenerators.Models;

internal readonly record struct CompilationInfo(
    EquatableArray<MapHandlerInvocationInfo> MapHandlerInvocationInfos
);
