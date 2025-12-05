using Microsoft.AspNetCore.Routing;

namespace AwsLambda.Host.Testing;

internal interface ILambdaRuntimeRouteManager
{
    bool TryMatch(
        HttpRequestMessage request,
        out RequestType? routeType,
        out RouteValueDictionary? values
    );
}
