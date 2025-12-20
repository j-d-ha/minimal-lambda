using System.Collections.Generic;
using System.Linq;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators;

internal static class DiagnosticGenerator
{
    internal static List<Diagnostic> GenerateDiagnostics(CompilationInfo compilationInfo)
    {
        var diagnostics = new List<Diagnostic>();

        var delegateInfos = compilationInfo.MapHandlerInvocationInfos;

        // Validate parameters
        foreach (var invocationInfo in delegateInfos)
            // check for multiple parameters that use the `[FromEvent]` attribute
            if (
                invocationInfo.DelegateInfo.Parameters.Count(p => p.Source == ParameterSource.Event)
                > 1
            )
                diagnostics.AddRange(
                    invocationInfo
                        .DelegateInfo.Parameters.Where(p => p.Source == ParameterSource.Event)
                        .Select(p =>
                            Diagnostic.Create(
                                Diagnostics.MultipleParametersUseAttribute,
                                p.LocationInfo?.ToLocation(),
                                AttributeConstants.FromEventAttribute
                            )
                        )
                );

        // check for invalid keyed service usage - MapHandler
        diagnostics.AddRange(
            compilationInfo.MapHandlerInvocationInfos.GenerateKeyedServiceKeyDiagnostics()
        );

        // check for invalid keyed service usage - OnShutdown
        diagnostics.AddRange(
            compilationInfo.OnShutdownInvocationInfos.GenerateKeyedServiceKeyDiagnostics()
        );

        foreach (var useMiddlewareTInfo in compilationInfo.UseMiddlewareTInfos)
        {
            // ensure middleware class is concrete
            if (useMiddlewareTInfo.ClassInfo.TypeKind is "interface" or "abstract class")
            {
                diagnostics.Add(
                    Diagnostic.Create(
                        Diagnostics.MustBeConcreteType,
                        useMiddlewareTInfo.GenericTypeArgumentLocation?.ToLocation(),
                        useMiddlewareTInfo.ClassInfo.ShortName
                    )
                );
            }

            // validate that middleware class constructors only use `[MiddlewareConstructor]` once
            diagnostics.AddRange(
                useMiddlewareTInfo
                    .ClassInfo.ConstructorInfos.Where(c =>
                        c.AttributeInfos.Any(a =>
                            a.FullName == AttributeConstants.MiddlewareConstructor
                        )
                    )
                    .Skip(1)
                    .Select(c =>
                        Diagnostic.Create(
                            Diagnostics.MultipleConstructorsWithAttribute,
                            c.AttributeInfos.First(a =>
                                    a.FullName == AttributeConstants.MiddlewareConstructor
                                )
                                .LocationInfo?.ToLocation(),
                            AttributeConstants.MiddlewareConstructor
                        )
                    )
            );
        }

        return diagnostics;
    }

    private static Diagnostic[] GenerateKeyedServiceKeyDiagnostics(
        this EquatableArray<HigherOrderMethodInfo> methodNameInfos
    ) =>
        methodNameInfos
            .SelectMany(onShutdownInvocationInfo =>
                onShutdownInvocationInfo.DelegateInfo.Parameters
            )
            .Where(parameterInfo => parameterInfo.KeyedServiceKey is { DisplayValue: null })
            .Select(parameterInfo =>
                Diagnostic.Create(
                    Diagnostics.InvalidAttributeArgument,
                    parameterInfo.KeyedServiceKey!.Value.LocationInfo?.ToLocation(),
                    parameterInfo.KeyedServiceKey.Value.Type
                )
            )
            .ToArray();
}
