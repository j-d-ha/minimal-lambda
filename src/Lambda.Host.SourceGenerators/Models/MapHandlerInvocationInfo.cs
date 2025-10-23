namespace Lambda.Host.SourceGenerators.Models;

internal readonly record struct MapHandlerInvocationInfo(
    DelegateInfo DelegateInfo,
    LocationInfo? LocationInfo,
    InterceptableLocationInfo InterceptableLocationInfo
);
