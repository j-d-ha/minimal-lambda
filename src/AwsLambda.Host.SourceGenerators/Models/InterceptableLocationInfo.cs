namespace AwsLambda.Host.SourceGenerators.Models;

internal readonly record struct InterceptableLocationInfo(
    int Version,
    string Data,
    string DisplayLocation
);
