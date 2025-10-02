using System.Collections.Generic;

namespace Lambda.Host.SourceGenerators.Models;

internal sealed class ParameterInfo
{
    internal required string ParameterName { get; set; }
    internal required string Type { get; set; }
    internal required LocationInfo? LocationInfo { get; set; }
    internal List<AttributeInfo> Attributes { get; set; } = [];
}
