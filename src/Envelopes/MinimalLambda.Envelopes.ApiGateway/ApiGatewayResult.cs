using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.ApiGateway;

public sealed class ApiGatewayResult : APIGatewayProxyResponse, IHttpResult<ApiGatewayResult>
{
    [JsonIgnore]
    private readonly IResponseEnvelope? _inner;

    private ApiGatewayResult(APIGatewayProxyResponse response)
    {
        _inner = response as IResponseEnvelope;
        StatusCode = response.StatusCode;
        Headers = response.Headers;
        MultiValueHeaders = response.MultiValueHeaders;
        Body = response.Body;
        IsBase64Encoded = response.IsBase64Encoded;
    }

    /// <inheritdoc />
    public void PackPayload(EnvelopeOptions options)
    {
        if (_inner is null)
            return;

        _inner.PackPayload(options);
        Body = ((APIGatewayProxyResponse)_inner).Body;
    }

    public ApiGatewayResult Customize(Action<ApiGatewayResult> customizer)
    {
        customizer(this);
        return this;
    }

    public static ApiGatewayResult Create<T>(
        int statusCode,
        T? bodyContent,
        string? body,
        IDictionary<string, string>? headers,
        bool isBase64Encoded
    ) =>
        new(
            new ApiGatewayResponseEnvelope<T>
            {
                StatusCode = statusCode,
                BodyContent = bodyContent,
                Body = body ?? string.Empty,
                Headers = headers,
                IsBase64Encoded = isBase64Encoded,
            }
        );
}
