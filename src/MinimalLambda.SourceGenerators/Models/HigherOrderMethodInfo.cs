using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Extensions;
using MinimalLambda.SourceGenerators.WellKnownTypes;
using WellKnownType = MinimalLambda.SourceGenerators.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace MinimalLambda.SourceGenerators.Models;

internal enum MethodType
{
    MapHandler,
    OnInit,
    OnShutdown,
}

internal interface IMethodInfo
{
    MethodType MethodType { get; }
    InterceptableLocationInfo InterceptableLocationInfo { get; }
    string DelegateCastType { get; }
    bool IsAwaitable { get; }
    bool HasResponse { get; }
    bool HasAnyFromKeyedServices { get; }
    EquatableArray<DiagnosticInfo> DiagnosticInfos { get; }
}

internal readonly record struct HigherOrderMethodInfo(
    InterceptableLocationInfo InterceptableLocationInfo,
    string InterceptableLocationAttribute,
    string DelegateCastType,
    EquatableArray<MapHandlerParameterInfo> ParameterAssignments,
    bool IsAwaitable,
    bool HasResponse,
    bool IsResponseTypeStream,
    bool IsResponseTypeBool,
    bool IsEventTypeStream,
    bool HasEvent,
    string? EventType,
    string? UnwrappedResponseType,
    bool HasAnyFromKeyedServices,
    EquatableArray<DiagnosticInfo> DiagnosticInfos,
    MethodType MethodType = MethodType.MapHandler
) : IMethodInfo;

internal static class HigherOrderMethodInfoExtensions
{
    extension(HigherOrderMethodInfo)
    {
        internal static HigherOrderMethodInfo Create(
            IMethodSymbol methodSymbol,
            GeneratorContext context
        )
        {
            var gotName = context.Node.TryGetMethodName(out var methodName);
            Debug.Assert(gotName, "Could not get method name. This should be unreachable");

            var handlerCastType = methodSymbol.GetCastableSignature();

            if (!InterceptableLocationInfo.TryGet(context, out var interceptableLocation))
                throw new InvalidOperationException("Unable to get interceptable location");

            var (assignments, diagnostics) = methodSymbol
                .Parameters.Select(parameter => MapHandlerParameterInfo.Create(parameter, context))
                .Aggregate(
                    (
                        Successes: new List<MapHandlerParameterInfo>(),
                        Diagnostics: new List<DiagnosticInfo>()
                    ),
                    static (acc, result) =>
                    {
                        result.Do(
                            info => acc.Successes.Add(info),
                            diagnostic => acc.Diagnostics.Add(diagnostic)
                        );

                        return acc;
                    },
                    static acc =>
                        (acc.Successes.ToEquatableArray(), acc.Diagnostics.ToEquatableArray())
                );

            var isAwaitable = methodSymbol.IsAwaitable(context);

            var hasResponse = methodSymbol.HasMeaningfulReturnType(context);

            var isReturnTypeStream =
                hasResponse
                && context.WellKnownTypes.IsTypeMatch(
                    methodSymbol.ReturnType,
                    WellKnownType.System_IO_Stream
                );

            var isReturnTypeBool =
                hasResponse
                && !isReturnTypeStream
                && context.WellKnownTypes.IsTypeMatch(
                    methodSymbol.ReturnType,
                    WellKnownType.System_Boolean
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

            return new HigherOrderMethodInfo
            {
                InterceptableLocationInfo = interceptableLocation.Value,
                InterceptableLocationAttribute =
                    interceptableLocation.Value.ToInterceptsLocationAttribute(),
                DelegateCastType = handlerCastType,
                ParameterAssignments = assignments,
                IsAwaitable = isAwaitable,
                HasResponse = hasResponse,
                IsResponseTypeStream = isReturnTypeStream,
                IsResponseTypeBool = isReturnTypeBool,
                IsEventTypeStream = isEventTypeStream,
                HasEvent = hasEvent,
                EventType = eventType,
                UnwrappedResponseType = unwrappedReturnType,
                HasAnyFromKeyedServices = hasAnyKeyedServices,
                DiagnosticInfos = diagnostics,
                MethodType = MethodType.MapHandler,
            };
        }
    }
}
