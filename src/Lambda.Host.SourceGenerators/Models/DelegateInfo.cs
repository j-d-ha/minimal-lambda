using Lambda.Host.SourceGenerators.Types;

namespace Lambda.Host.SourceGenerators.Models;

internal readonly record struct DelegateInfo(
    string ResponseType = TypeConstants.Void,
    EquatableArray<ParameterInfo> Parameters = new()
)
{
    internal string DelegateType =>
        ResponseType == TypeConstants.Void ? TypeConstants.Action : TypeConstants.Func;
}
