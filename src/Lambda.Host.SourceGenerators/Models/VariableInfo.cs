namespace Lambda.Host.SourceGenerators.Models;

internal record VariableInfo
{
    internal required string Type { get; init; }
    internal required string Name { get; init; }
}
