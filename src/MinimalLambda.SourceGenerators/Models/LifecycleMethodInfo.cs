using System;
using System.Collections.Generic;
using System.Linq;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Extensions;
using MinimalLambda.SourceGenerators.WellKnownTypes;

namespace MinimalLambda.SourceGenerators.Models;

internal record LifecycleMethodInfo(
    EquatableArray<DiagnosticInfo> DiagnosticInfos,
    MethodType MethodType
) : IMethodInfo;

internal static class LifecycleMethodInfoExtensions
{
    extension(LifecycleMethodInfo)
    {
        internal static LifecycleMethodInfo Create(
            IMethodSymbol methodSymbol,
            MethodType methodType,
            GeneratorContext context
        )
        {
            var handlerCastType = methodSymbol.GetCastableSignature();

            if (!InterceptableLocationInfo.TryGet(context, out var interceptableLocation))
                throw new InvalidOperationException("Unable to get interceptable location");

            var (assignments, diagnostics) = methodSymbol.Parameters.CollectDiagnosticResults(
                parameter => MapHandlerParameterInfo.Create(parameter, context)
            );

            var isAwaitable = methodSymbol.IsAwaitable(context);

            var hasResponse = methodSymbol.HasMeaningfulReturnType(context);

            var isReturnTypeStream =
                hasResponse
                && context.WellKnownTypes.IsTypeMatch(
                    methodSymbol.ReturnType,
                    WellKnownTypeData.WellKnownType.System_IO_Stream
                );

            var hasEvent = assignments.Any(a => a.IsEvent);

            var eventType = hasEvent
                ? assignments.Where(a => a.IsEvent).Select(a => a.GloballyQualifiedType).First()
                : null;

            var isEventTypeStream =
                hasEvent && assignments.Any(a => a is { IsEvent: true, IsStream: true });

            var hasAnyKeyedServices = assignments.Any(a => a is { IsFromKeyedService: true });

            var unwrappedReturnType = methodSymbol
                .UnwrapReturnType(context)
                .ToGloballyQualifiedName();

            return new LifecycleMethodInfo(
                MethodType: methodType,
                // InterceptableLocationInfo: interceptableLocation.Value,
                // InterceptableLocationAttribute:
                // interceptableLocation.Value.ToInterceptsLocationAttribute(),
                // DelegateCastType: handlerCastType,
                // ParameterAssignments: assignments.ToEquatableArray(),
                // IsAwaitable: isAwaitable,
                // HasResponse: hasResponse,
                // IsResponseTypeStream: isReturnTypeStream,
                // IsEventTypeStream: isEventTypeStream,
                // HasEvent: hasEvent,
                // EventType: eventType,
                // UnwrappedResponseType: unwrappedReturnType,
                // HasAnyFromKeyedServices: hasAnyKeyedServices,
                DiagnosticInfos: diagnostics.ToEquatableArray()
            );
        }
    }
}
