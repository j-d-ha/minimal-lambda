using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace AwsLambda.Host.Testing;

internal class LambdaRuntimeRouteManager : ILambdaRuntimeRouteManager
{
    private static readonly RouteTemplate[] Routes =
    [
        new()
        {
            Type = RequestType.GetNextInvocation,
            Method = HttpMethod.Get.Method,
            Matcher = new TemplateMatcher(
                TemplateParser.Parse("2018-06-01/runtime/invocation/next"),
                new RouteValueDictionary()
            ),
        },
        new()
        {
            Type = RequestType.PostResponse,
            Method = HttpMethod.Post.Method,
            Matcher = new TemplateMatcher(
                TemplateParser.Parse("2018-06-01/runtime/invocation/{requestId}/response"),
                new RouteValueDictionary()
            ),
        },
        new()
        {
            Type = RequestType.PostError,
            Method = HttpMethod.Post.Method,
            Matcher = new TemplateMatcher(
                TemplateParser.Parse("2018-06-01/runtime/invocation/{requestId}/error"),
                new RouteValueDictionary()
            ),
        },
    ];

    public bool TryMatch(
        HttpRequestMessage request,
        [NotNullWhen(true)] out RequestType? routeType,
        [NotNullWhen(true)] out RouteValueDictionary? values
    )
    {
        routeType = null;
        values = null;

        var method = request.Method.Method;
        var path = request.RequestUri?.AbsolutePath.TrimStart('/') ?? string.Empty;

        foreach (var route in Routes)
        {
            if (route.Method != method)
                continue;

            var routeValues = new RouteValueDictionary();
            if (route.Matcher.TryMatch(path, routeValues))
            {
                routeType = route.Type;
                values = routeValues;
                return true;
            }
        }

        return false;
    }

    private class RouteTemplate
    {
        internal required TemplateMatcher Matcher { get; init; }
        internal required string Method { get; init; }
        internal required RequestType Type { get; init; }
    }
}
