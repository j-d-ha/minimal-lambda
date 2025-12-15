namespace MinimalLambda.Envelopes.ApiGateway;

public static class UpdateHttpResultExtensions
{
    extension<THttpResult>(THttpResult result)
        where THttpResult : IHttpResult<THttpResult>
    {
        public THttpResult AddHeader(string key, string value)
        {
            result.Headers[key] = value;
            return result;
        }

        public THttpResult AddContentType(string contentType) =>
            result.AddHeader("Content-Type", contentType);
    }
}
