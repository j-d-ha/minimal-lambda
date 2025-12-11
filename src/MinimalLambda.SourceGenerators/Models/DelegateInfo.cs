using System.Linq;
using MinimalLambda.SourceGenerators.Types;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct DelegateInfo(
    EquatableArray<ParameterInfo> Parameters,
    bool IsAwaitable,
    bool IsAsync,
    TypeInfo ReturnTypeInfo
)
{
    internal readonly ParameterInfo? EventParameter = GetEventParameter(Parameters);

    internal string DelegateType =>
        ReturnTypeInfo.FullyQualifiedType == TypeConstants.Void
            ? TypeConstants.Action
            : TypeConstants.Func;

    internal bool HasAnyKeyedServiceParameter =>
        Parameters.Any(p => p.Source == ParameterSource.KeyedService);

    internal bool HasEventParameter => EventParameter is not null;

    internal bool HasResponse =>
        ReturnTypeInfo.FullyQualifiedType
            is not (TypeConstants.Void or TypeConstants.Task or TypeConstants.ValueTask);

    private static ParameterInfo? GetEventParameter(EquatableArray<ParameterInfo> parameters) =>
        parameters
            .Where(p => p.Source == ParameterSource.Event)
            .Select(p => (ParameterInfo?)p)
            .FirstOrDefault();
}
