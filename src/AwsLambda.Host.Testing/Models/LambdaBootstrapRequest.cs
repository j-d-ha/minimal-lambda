using Microsoft.AspNetCore.Routing;

namespace AwsLambda.Host.Testing;

internal class LambdaBootstrapRequest
{
    internal required RequestType RequestType { get; init; }
    internal required HttpRequestMessage RequestMessage { get; init; }
    internal required RouteValueDictionary RouteValue { get; init; }

    internal string? RequestId
    {
        get
        {
            field ??=
                RouteValue.TryGetValue("RequestId", out var requestId)
                && requestId is string requestIdString
                    ? requestIdString
                    : null;

            return field;
        }
    }
}
