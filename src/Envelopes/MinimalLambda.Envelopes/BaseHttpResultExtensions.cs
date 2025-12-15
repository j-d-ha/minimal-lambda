namespace MinimalLambda.Envelopes.ApiGateway;

public static class BaseHttpResultExtensions
{
    extension<THttpResult>(IHttpResult<THttpResult>)
        where THttpResult : IHttpResult<THttpResult>
    {
        public static THttpResult StatusCode(int statusCode) =>
            THttpResult.Create<object?>(
                statusCode,
                null,
                null,
                new Dictionary<string, string>(),
                false
            );

        public static THttpResult Text(int statusCode, string body) =>
            THttpResult.Create<object?>(
                statusCode,
                null,
                body,
                new Dictionary<string, string> { ["Content-Type"] = "text/plain; charset=utf-8" },
                false
            );

        public static THttpResult Json<T>(int statusCode, T bodyContent) =>
            THttpResult.Create(
                statusCode,
                bodyContent,
                null,
                new Dictionary<string, string>
                {
                    ["Content-Type"] = "application/json; charset=utf-8",
                },
                false
            );
    }
}
