using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MinimalLambda.SourceGenerators.Models;

internal readonly record struct InterceptableLocationInfo(
    int Version,
    string Data,
    string DisplayLocation
);

internal static class InterceptableLocationInfoExtensions
{
    extension(InterceptableLocationInfo location)
    {
        internal static InterceptableLocationInfo CreateFrom(
            InterceptableLocation interceptableLocation
        ) =>
            new(
                interceptableLocation.Version,
                interceptableLocation.Data,
                interceptableLocation.GetDisplayLocation()
            );

        internal static bool TryGet(
            GeneratorContext context,
            [NotNullWhen(true)] out InterceptableLocationInfo? interceptableLocationInfo
        )
        {
            interceptableLocationInfo = null;

            if (context.Node is not InvocationExpressionSyntax invocationExpr)
                return false;

            var interceptableLocation = context.SemanticModel.GetInterceptableLocation(
                invocationExpr,
                context.CancellationToken
            );

            if (interceptableLocation is null)
                return false;

            interceptableLocationInfo = InterceptableLocationInfo.CreateFrom(interceptableLocation);
            return true;
        }

        internal string ToInterceptsLocationAttribute() =>
            $"""[InterceptsLocation({location.Version}, "{location.Data}")]""";
    }

    extension(InterceptableLocation interceptableLocation)
    {
        internal InterceptableLocationInfo ToInterceptableLocationInfo() =>
            InterceptableLocationInfo.CreateFrom(interceptableLocation);
    }
}
