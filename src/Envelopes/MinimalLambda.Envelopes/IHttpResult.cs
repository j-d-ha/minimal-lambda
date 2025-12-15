namespace MinimalLambda.Envelopes;

/// <summary>
///     Defines the contract for HTTP response results returned from ALB, API Gateway v1, and API
///     Gateway v2 Lambda integrations.
/// </summary>
/// <typeparam name="TSelf">The concrete implementing type for fluent method chaining.</typeparam>
public interface IHttpResult<out TSelf> : IResponseEnvelope
    where TSelf : IHttpResult<TSelf>
{
    /// <summary>Gets or sets the response body content.</summary>
    public string Body { get; set; }

    /// <summary>Gets or sets the HTTP response headers.</summary>
    public IDictionary<string, string> Headers { get; set; }

    /// <summary>Gets or sets whether the body is base64-encoded for binary content.</summary>
    public bool IsBase64Encoded { get; set; }

    /// <summary>Gets or sets the HTTP status code.</summary>
    public int StatusCode { get; set; }

    /// <summary>Creates a new HTTP result instance.</summary>
    /// <remarks>
    ///     Provide either <paramref name="bodyContent" /> for automatic serialization or
    ///     <paramref name="body" /> for pre-serialized content.
    /// </remarks>
    /// <typeparam name="TResponse">The type of content being returned.</typeparam>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="bodyContent">The typed content to serialize into the body.</param>
    /// <param name="body">A pre-serialized body string.</param>
    /// <param name="headers">Optional response headers.</param>
    /// <param name="isBase64Encoded">Whether the body is base64-encoded.</param>
    /// <returns>A new HTTP result instance.</returns>
    static abstract TSelf Create<TResponse>(
        int statusCode,
        TResponse? bodyContent,
        string? body,
        IDictionary<string, string>? headers,
        bool isBase64Encoded
    );
}
