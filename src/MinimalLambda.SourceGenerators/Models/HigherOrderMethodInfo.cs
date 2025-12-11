using System.Collections.Immutable;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct HigherOrderMethodInfo(
    string Name,
    DelegateInfo DelegateInfo,
    LocationInfo? LocationInfo,
    InterceptableLocationInfo InterceptableLocationInfo,
    ImmutableArray<ArgumentInfo> ArgumentsInfos
);
