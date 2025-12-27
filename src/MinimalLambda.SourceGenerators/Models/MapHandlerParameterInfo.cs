using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Extensions;
using MinimalLambda.SourceGenerators.WellKnownTypes;
using WellKnownType = MinimalLambda.SourceGenerators.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct MapHandlerParameterInfo(
    string GloballyQualifiedType,
    bool IsStream,
    string Assignment,
    string InfoComment,
    bool IsEvent,
    bool IsFromKeyedService,
    LocationInfo? LocationInfo
);

internal static class MapHandlerParameterInfoExtensions
{
    extension(MapHandlerParameterInfo)
    {
        internal static DiagnosticResult<MapHandlerParameterInfo> Create(
            IParameterSymbol parameter,
            GeneratorContext context
        )
        {
            var paramType = parameter.Type.ToGloballyQualifiedName();

            var parameterInfo = new MapHandlerParameterInfo
            {
                GloballyQualifiedType = parameter.Type.ToGloballyQualifiedName(),
                IsStream = context.WellKnownTypes.IsTypeMatch(
                    parameter.Type,
                    WellKnownType.System_IO_Stream
                ),
                IsEvent = false,
                IsFromKeyedService = false,
                LocationInfo = LocationInfo.Create(parameter),
            };

            // event
            if (parameter.IsFromEvent(context))
                return DiagnosticResult<MapHandlerParameterInfo>.Success(
                    parameterInfo with
                    {
                        Assignment = parameterInfo.IsStream
                            // stream event
                            ? "context.Features.GetRequired<IInvocationDataFeature>().EventStream"
                            // non stream event
                            : $"context.GetRequiredEvent<{paramType}>()",
                        IsEvent = true,
                    }
                );

            // context
            if (
                context.WellKnownTypes.IsAnyTypeMatch(
                    parameter.Type,
                    WellKnownType.Amazon_Lambda_Core_ILambdaContext,
                    WellKnownType.MinimalLambda_ILambdaInvocationContext
                )
            )
                return DiagnosticResult<MapHandlerParameterInfo>.Success(
                    parameterInfo with
                    {
                        Assignment = "context",
                    }
                );

            // cancellation token
            if (
                context.WellKnownTypes.IsTypeMatch(
                    parameter.Type,
                    WellKnownType.System_Threading_CancellationToken
                )
            )
                return DiagnosticResult<MapHandlerParameterInfo>.Success(
                    parameterInfo with
                    {
                        Assignment = "context.CancellationToken",
                    }
                );

            // default assignment from Di
            return parameter
                .GetDiParameterAssignment(context)
                .Bind(diInfo =>
                    DiagnosticResult<MapHandlerParameterInfo>.Success(
                        parameterInfo with
                        {
                            Assignment = diInfo.Assignment,
                            IsFromKeyedService = diInfo.IsKeyed,
                        }
                    )
                );
        }
    }
}
