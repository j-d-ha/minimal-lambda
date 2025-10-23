using Lambda.Host.SourceGenerators.Types;

namespace Lambda.Host.SourceGenerators.Models;

internal readonly record struct ParameterInfo(
    string Type,
    LocationInfo? LocationInfo,
    EquatableArray<AttributeInfo> Attributes
);
