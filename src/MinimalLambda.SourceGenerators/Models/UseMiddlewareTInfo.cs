namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct UseMiddlewareTInfo(
    InterceptableLocationInfo InterceptableLocationInfo,
    ClassInfo ClassInfo,
    LocationInfo? GenericTypeArgumentLocation
);
