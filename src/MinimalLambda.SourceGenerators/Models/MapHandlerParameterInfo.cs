using Microsoft.CodeAnalysis;
using MinimalLambda.SourceGenerators.Extensions;
using WellKnownType = MinimalLambda.SourceGenerators.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct MapHandlerParameterInfo(
    string GloballyQualifiedType,
    bool IsStream,
    string Assignment,
    string InfoComment,
    bool IsEvent,
    bool IsFromKeyedService
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
            var stream = context.WellKnownTypes.Get(WellKnownType.System_IO_Stream);
            var lambdaContext = context.WellKnownTypes.Get(
                WellKnownType.Amazon_Lambda_Core_ILambdaContext
            );
            var lambdaInvocationContext = context.WellKnownTypes.Get(
                WellKnownType.MinimalLambda_ILambdaInvocationContext
            );
            var cancellationToken = context.WellKnownTypes.Get(
                WellKnownType.System_Threading_CancellationToken
            );

            var isStream = parameter.Type.Equals(stream, SymbolEqualityComparer.Default);

            var paramType = parameter.Type.ToGloballyQualifiedName();

            var parameterInfo = new MapHandlerParameterInfo
            {
                GloballyQualifiedType = parameter.Type.ToGloballyQualifiedName(),
                IsStream = isStream,
                IsEvent = false,
                IsFromKeyedService = false,
            };

            // event
            if (parameter.IsFromEvent(context))
                return DiagnosticResult<MapHandlerParameterInfo>.Success(
                    parameterInfo with
                    {
                        Assignment = isStream
                            // stream event
                            ? "context.Features.GetRequired<IInvocationDataFeature>().EventStream"
                            // non stream event
                            : $"context.GetRequiredEvent<{paramType}>()",
                        IsEvent = true,
                    }
                );

            // context
            if (
                SymbolEqualityComparer.Default.Equals(parameter.Type, lambdaContext)
                || SymbolEqualityComparer.Default.Equals(parameter.Type, lambdaInvocationContext)
            )
                return DiagnosticResult<MapHandlerParameterInfo>.Success(
                    parameterInfo with
                    {
                        Assignment = "context",
                    }
                );

            // cancellation token
            if (SymbolEqualityComparer.Default.Equals(parameter.Type, cancellationToken))
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
