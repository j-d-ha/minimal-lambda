using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace MinimalLambda.Testing;

internal class LambdaRuntimeRouteManager
{
    private static readonly RouteTemplate[] Routes =
    [
        new()
        {
            Type = RequestType.GetNextInvocation,
            Method = HttpMethod.Get.Method,
            Matcher = new TemplateMatcher(
                TemplateParser.Parse("{version}/runtime/invocation/next"),
                new RouteValueDictionary()
            ),
        },
        new()
        {
            Type = RequestType.PostInitError,
            Method = HttpMethod.Post.Method,
            Matcher = new TemplateMatcher(
                TemplateParser.Parse("{version}/runtime/init/error"),
                new RouteValueDictionary()
            ),
        },
        new()
        {
            Type = RequestType.PostResponse,
            Method = HttpMethod.Post.Method,
            Matcher = new TemplateMatcher(
                TemplateParser.Parse("{version}/runtime/invocation/{requestId}/response"),
                new RouteValueDictionary()
            ),
        },
        new()
        {
            Type = RequestType.PostError,
            Method = HttpMethod.Post.Method,
            Matcher = new TemplateMatcher(
                TemplateParser.Parse("{version}/runtime/invocation/{requestId}/error"),
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
        // var path = request.RequestUri?.AbsolutePath.TrimStart('/') ?? string.Empty;
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;

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
