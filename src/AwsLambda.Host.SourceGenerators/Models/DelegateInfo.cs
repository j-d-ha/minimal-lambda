using System.Linq;
using AwsLambda.Host.SourceGenerators.Types;

namespace AwsLambda.Host.SourceGenerators.Models;

internal readonly record struct DelegateInfo(
    string FullResponseType,
    string? UnwrappedResponseType,
    EquatableArray<ParameterInfo> Parameters,
    bool IsAwaitable,
    bool IsAsync
)
{
    internal readonly ParameterInfo? EventParameter = GetEventParameter(Parameters);

    internal string DelegateType =>
        FullResponseType == TypeConstants.Void ? TypeConstants.Action : TypeConstants.Func;

    internal bool HasResponse =>
        FullResponseType
            is not (TypeConstants.Void or TypeConstants.Task or TypeConstants.ValueTask);

    internal bool HasEventParameter => EventParameter is not null;

    private static ParameterInfo? GetEventParameter(EquatableArray<ParameterInfo> parameters) =>
        parameters
            .Where(p => p.Source == ParameterSource.Event)
            .Select(p => (ParameterInfo?)p)
            .FirstOrDefault();
}
