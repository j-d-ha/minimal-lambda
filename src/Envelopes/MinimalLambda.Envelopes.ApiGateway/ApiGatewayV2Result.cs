using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.ApiGateway;

/// <summary>
///     Represents an HTTP response for AWS Lambda functions invoked by Amazon API Gateway HTTP
///     API (v2).
/// </summary>
/// <remarks>
///     This class wraps <see cref="APIGatewayHttpApiV2ProxyResponse" /> and provides support for
///     response envelope customization through <see cref="IHttpResult{TSelf}" />. Use this type when
///     returning responses from Lambda proxy integrations with API Gateway HTTP APIs.
/// </remarks>
public sealed class ApiGatewayV2Result
    : APIGatewayHttpApiV2ProxyResponse, IHttpResult<ApiGatewayV2Result>
{
    [JsonIgnore]
    private readonly IResponseEnvelope? _inner;

    private ApiGatewayV2Result(APIGatewayHttpApiV2ProxyResponse response)
    {
        _inner = response as IResponseEnvelope;
        StatusCode = response.StatusCode;
        Headers = response.Headers;
        Cookies = response.Cookies;
        Body = response.Body;
        IsBase64Encoded = response.IsBase64Encoded;
    }

    /// <inheritdoc />
    public void PackPayload(EnvelopeOptions options)
    {
        if (_inner is null)
            return;

        _inner.PackPayload(options);
        Body = ((APIGatewayHttpApiV2ProxyResponse)_inner).Body;
    }

    /// <inheritdoc />
    public static ApiGatewayV2Result Create<T>(
        int statusCode,
        T? bodyContent,
        IDictionary<string, string> headers,
        bool isBase64Encoded) =>
        new(
            new ApiGatewayV2ResponseEnvelope<T>
            {
                StatusCode = statusCode,
                BodyContent = bodyContent,
                Headers = headers,
                IsBase64Encoded = isBase64Encoded,
            });

    /// <summary>Creates an API Gateway v2 result from an existing response envelope.</summary>
    /// <typeparam name="T">The type of content in the envelope's body.</typeparam>
    /// <param name="envelope">The response envelope to wrap.</param>
    /// <returns>An <see cref="ApiGatewayV2Result" /> wrapping the envelope.</returns>
    public static ApiGatewayV2Result Create<T>(ApiGatewayV2ResponseEnvelope<T> envelope) =>
        new(envelope);
}
