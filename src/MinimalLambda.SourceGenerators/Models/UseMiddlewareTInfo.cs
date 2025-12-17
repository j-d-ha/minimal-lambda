using MinimalLambda.SourceGenerators.Types;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct UseMiddlewareTInfo(
    InterceptableLocationInfo InterceptableLocationInfo,
    ClassInfo ClassInfo,
    EquatableArray<ConstructorInfo> ConstructorInfos
);

internal readonly record struct ClassInfo(string GloballyQualifiedName);

internal readonly record struct ConstructorInfo(int ArgumentCount);
