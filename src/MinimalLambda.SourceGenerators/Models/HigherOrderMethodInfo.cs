using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Extensions;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct HigherOrderMethodInfo(
    string Name,
    DelegateInfo DelegateInfo,
    LocationInfo? LocationInfo,
    InterceptableLocationInfo InterceptableLocationInfo,
    ImmutableArray<ArgumentInfo> ArgumentsInfos,
    // ── New ──────────────────────────────────────────────────────────────────────────
    string DelegateCastType = ""
);

internal static class HigherOrderMethodInfoExtensions
{
    extension(HigherOrderMethodInfo)
    {
        internal static HigherOrderMethodInfo? Create(
            IMethodSymbol methodSymbol,
            string name,
            GeneratorContext context
        )
        {
            var handlerCastType = GetMethodSignature(methodSymbol);

            return null;
        }
    }

    private static string GetMethodSignature(IMethodSymbol method)
    {
        var returnType = method.ReturnType.ToGloballyQualifiedName();
        var parameters = method
            .Parameters.Select(
                (p, i) =>
                {
                    var type = p.Type.ToGloballyQualifiedName();
                    var defaultValue = p.IsOptional ? " = default" : "";
                    return $"{type} arg{i}{defaultValue}";
                }
            )
            .ToArray();
        var parameterList = string.Join(", ", parameters);

        return $"{returnType} ({parameterList}) => throw null!";
    }
}
