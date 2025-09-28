namespace Lambda.Host.SourceGenerators.Models;

internal record DependencyInfo
{
    internal required string Type { get; init; }
    internal required string ParameterName { get; init; }
    internal string FieldName => "_" + ParameterName;
}
