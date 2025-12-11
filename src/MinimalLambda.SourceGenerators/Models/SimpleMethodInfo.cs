namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct SimpleMethodInfo(
    string Name,
    LocationInfo? LocationInfo,
    InterceptableLocationInfo InterceptableLocationInfo
);
