using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using LayeredCraft.SourceGeneratorTools.Types;
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
    string DelegateCastType = "",
    EquatableArray<string> ParameterAssignments = default,
    EquatableArray<DiagnosticInfo> DiagnosticInfos = default
);

internal static class HigherOrderMethodInfoExtensions
{
    extension(HigherOrderMethodInfo)
    {
        internal static HigherOrderMethodInfo? Create(
            IMethodSymbol methodSymbol,
            string name,
            Func<
                IMethodSymbol,
                GeneratorContext,
                IEnumerable<(string?, DiagnosticInfo?)>
            > getParameterAssignments,
            GeneratorContext context
        )
        {
            var handlerCastType = GetMethodSignature(methodSymbol);
            var location = context.Node.CreateLocationInfo();

            if (!InterceptableLocationInfo.TryGet(context, out var interceptableLocation))
                throw new InvalidOperationException("Unable to get interceptable location");

            var (assignments, diagnostics) = getParameterAssignments(methodSymbol, context)
                .Aggregate(
                    (Successes: new List<string>(), Diagnostics: new List<DiagnosticInfo>()),
                    static (acc, result) =>
                    {
                        if (result.Item1 is not null)
                            acc.Successes.Add(result.Item1);

                        if (result.Item2 is not null)
                            acc.Diagnostics.Add(result.Item2.Value);

                        return acc;
                    },
                    static acc => (acc.Successes.ToEquatableArray(), acc.Diagnostics.ToArray())
                );

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
