using AwsLambda.Host.SourceGenerators.Types;

namespace AwsLambda.Host.SourceGenerators.Models;

internal readonly record struct ParameterInfo(
    string Type,
    LocationInfo? LocationInfo,
    EquatableArray<AttributeInfo> Attributes
);
