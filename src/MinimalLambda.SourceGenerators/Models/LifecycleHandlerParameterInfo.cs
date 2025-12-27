using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Extensions;
using MinimalLambda.SourceGenerators.WellKnownTypes;
using WellKnownType = MinimalLambda.SourceGenerators.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace MinimalLambda.SourceGenerators.Models;

internal record LifecycleHandlerParameterInfo(
    string Assignment,
    string InfoComment,
    bool IsFromKeyedService,
    LocationInfo? LocationInfo,
    MapHandlerParameterSource Source,
    string? KeyedServicesKey
);

internal static class LifecycleHandlerParameterInfoExtensions
{
    extension(LifecycleHandlerParameterInfo)
    {
        internal static DiagnosticResult<LifecycleHandlerParameterInfo> Create(
            IParameterSymbol parameter,
            GeneratorContext context
        )
        {
            var parameterInfo = new LifecycleHandlerParameterInfo(
                IsFromKeyedService: false,
                LocationInfo: LocationInfo.Create(parameter),
                Assignment: string.Empty,
                InfoComment: string.Empty,
                KeyedServicesKey: string.Empty,
                Source: MapHandlerParameterSource.Services
            );

            // context
            if (
                context.WellKnownTypes.IsAnyTypeMatch(
                    parameter.Type,
                    WellKnownType.MinimalLambda_ILambdaLifecycleContext
                )
            )
                return DiagnosticResult<LifecycleHandlerParameterInfo>.Success(
                    parameterInfo with
                    {
                        Assignment = "context",
                        Source = MapHandlerParameterSource.Context,
                    }
                );

            // cancellation token
            if (
                context.WellKnownTypes.IsTypeMatch(
                    parameter.Type,
                    WellKnownType.System_Threading_CancellationToken
                )
            )
                return DiagnosticResult<LifecycleHandlerParameterInfo>.Success(
                    parameterInfo with
                    {
                        Assignment = "context.CancellationToken",
                        Source = MapHandlerParameterSource.CancellationToken,
                    }
                );

            // default assignment from Di
            return parameter
                .GetDiParameterAssignment(context)
                .Bind(diInfo =>
                    DiagnosticResult<LifecycleHandlerParameterInfo>.Success(
                        parameterInfo with
                        {
                            Assignment = diInfo.Assignment,
                            IsFromKeyedService = diInfo.Key is not null,
                            Source = diInfo.Key is not null
                                ? MapHandlerParameterSource.KeyedServices
                                : MapHandlerParameterSource.Services,
                            KeyedServicesKey = diInfo.Key,
                        }
                    )
                );
        }
    }
}
