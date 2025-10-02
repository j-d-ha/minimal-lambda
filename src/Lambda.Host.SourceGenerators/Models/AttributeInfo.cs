using System.Collections.Generic;

namespace Lambda.Host.SourceGenerators.Models;

internal sealed class AttributeInfo
{
    internal required string? Type { get; set; }
    internal List<string> Arguments { get; set; } = [];
}
