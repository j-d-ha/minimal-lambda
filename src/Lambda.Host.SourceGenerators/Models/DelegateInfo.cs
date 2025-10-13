using System.Collections.Immutable;

namespace Lambda.Host.SourceGenerators.Models;

internal readonly record struct DelegateInfo(
    string? Namespace,
    bool IsAsync,
    string? ResponseType = TypeConstants.Void,
    ImmutableArray<ParameterInfo> Parameters = new()
)
{
    internal string DelegateType =>
        ResponseType == TypeConstants.Void ? TypeConstants.Action : TypeConstants.Func;
}
