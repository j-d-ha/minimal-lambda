using System.Collections.Immutable;

namespace Lambda.Host.SourceGenerators.Models;

internal readonly record struct ParameterInfo(
    string ParameterName,
    string Type,
    LocationInfo? LocationInfo,
    ImmutableArray<AttributeInfo> Attributes
);
