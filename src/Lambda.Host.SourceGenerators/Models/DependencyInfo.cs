namespace Lambda.Host.SourceGenerators.Models;

internal readonly record struct DependencyInfo(string Type, string ParameterName)
{
    internal string FieldName => "_" + ParameterName;
    internal string InternalVariableName => "__" + ParameterName;
}
