using System.Collections.Immutable;

namespace AwsLambda.Host.SourceGenerators.Models;

internal readonly record struct HigherOrderMethodInfo(
    DelegateInfo DelegateInfo,
    LocationInfo? LocationInfo,
    InterceptableLocationInfo InterceptableLocationInfo,
    ImmutableArray<GenericInfo> GenericTypeArguments = new()
);
