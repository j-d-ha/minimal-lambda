using System.Collections.Immutable;

namespace AwsLambda.Host.SourceGenerators.Models;

internal readonly record struct HigherOrderMethodInfo(
    string Name,
    DelegateInfo DelegateInfo,
    LocationInfo? LocationInfo,
    InterceptableLocationInfo InterceptableLocationInfo,
    ImmutableArray<GenericInfo> GenericTypeArguments = new()
);
