using Microsoft.CodeAnalysis.CSharp;

namespace AwsLambda.Host.SourceGenerators.Models;

internal readonly record struct InterceptableLocationInfo(
    int Version,
    string Data,
    string DisplayLocation
)
{
    internal static InterceptableLocationInfo CreateFrom(
        InterceptableLocation interceptableLocation
    ) =>
        new(
            interceptableLocation.Version,
            interceptableLocation.Data,
            interceptableLocation.GetDisplayLocation()
        );
}
