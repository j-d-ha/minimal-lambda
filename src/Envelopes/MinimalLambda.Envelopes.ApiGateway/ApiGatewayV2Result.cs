using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.ApiGateway;

public sealed class ApiGatewayV2Result
    : APIGatewayHttpApiV2ProxyResponse,
        IHttpResult<ApiGatewayV2Result>
{
    [JsonIgnore]
    private readonly IResponseEnvelope? _inner;

    private ApiGatewayV2Result(APIGatewayHttpApiV2ProxyResponse response)
    {
        _inner = response as IResponseEnvelope;
        StatusCode = response.StatusCode;
        Headers = response.Headers;
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

    public static ApiGatewayV2Result Create<T>(
        int statusCode,
        T? bodyContent,
        string? body,
        IDictionary<string, string>? headers,
        bool isBase64Encoded
    ) =>
        new(
            new ApiGatewayV2ResponseEnvelope<T>
            {
                StatusCode = statusCode,
                BodyContent = bodyContent,
                Body = body ?? string.Empty,
                Headers = headers,
                IsBase64Encoded = isBase64Encoded,
            }
        );
}
