namespace AwsLambda.Host.SourceGenerators.Models;

internal readonly record struct ParameterInfo(
    string Type,
    LocationInfo? LocationInfo,
    ParameterSource Source,
    string? KeyedServiceKey
);
