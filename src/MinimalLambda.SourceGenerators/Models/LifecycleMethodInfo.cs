using System;
using System.Collections.Generic;
using System.Linq;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;

namespace MinimalLambda.SourceGenerators.Models;

internal record LifecycleMethodInfo(
    string InterceptableLocationAttribute,
    string DelegateCastType,
    EquatableArray<DiagnosticInfo> DiagnosticInfos,
    MethodType MethodType,
    string HandleResponseAssignment,
    string HandleReturningFromMethod
) : IMethodInfo;

internal static class LifecycleMethodInfoExtensions
{
    extension(LifecycleMethodInfo)
    {
        internal static LifecycleMethodInfo CreateForInit(
            IMethodSymbol methodSymbol,
            GeneratorContext context
        )
        {
            var handlerCastType = methodSymbol.GetCastableSignature();

            if (!InterceptableLocationInfo.TryGet(context, out var interceptableLocation))
                throw new InvalidOperationException("Unable to get interceptable location");

            var (assignments, diagnostics) = methodSymbol.Parameters.CollectDiagnosticResults(
                parameter => LifecycleHandlerParameterInfo.Create(parameter, context)
            );

            var isAwaitable = methodSymbol.IsAwaitable(context);

            var hasResponse = methodSymbol.HasMeaningfulReturnType(
                context,
                out var unwrappedReturnType
            );

            var hasAnyKeyedServices = assignments.Any(a => a is { IsFromKeyedService: true });

            // var unwrappedReturnType = methodSymbol
            //     .UnwrapReturnType(context)
            //     .ToGloballyQualifiedName();

            return new LifecycleMethodInfo(
                MethodType: MethodType.OnInit,
                InterceptableLocationAttribute: interceptableLocation.Value.ToInterceptsLocationAttribute(),
                DelegateCastType: handlerCastType,
                DiagnosticInfos: diagnostics.ToEquatableArray(),
                HandleResponseAssignment: "",
                HandleReturningFromMethod: ""
            );
        }

        internal static LifecycleMethodInfo CreateForShutdown(
            IMethodSymbol methodSymbol,
            GeneratorContext context
        ) => throw new NotImplementedException();
    }
}
