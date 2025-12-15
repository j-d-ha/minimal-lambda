namespace MinimalLambda.Envelopes.ApiGateway;

public interface IHttpResult<out TSelf> : IResponseEnvelope
    where TSelf : IHttpResult<TSelf>
{
    public int StatusCode { get; set; }

    public IDictionary<string, string> Headers { get; set; }

    public string Body { get; set; }

    public bool IsBase64Encoded { get; set; }

    static abstract TSelf Create<TResponse>(
        int statusCode,
        TResponse? bodyContent,
        string? body,
        IDictionary<string, string>? headers,
        bool isBase64Encoded
    );
}
