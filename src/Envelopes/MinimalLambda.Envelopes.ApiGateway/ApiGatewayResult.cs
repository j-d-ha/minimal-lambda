using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.ApiGateway;

/// <summary>
///     Represents an HTTP response for AWS Lambda functions invoked by Amazon API Gateway REST
///     API (v1).
/// </summary>
/// <remarks>
///     This class wraps <see cref="APIGatewayProxyResponse" /> and provides support for response
///     envelope customization through <see cref="IHttpResult{TSelf}" />. Use this type when returning
///     responses from Lambda proxy integrations with API Gateway REST APIs.
/// </remarks>
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

    /// <inheritdoc />
    public static ApiGatewayResult Create<T>(
        int statusCode,
        T? bodyContent,
        IDictionary<string, string> headers,
        bool isBase64Encoded
    ) =>
        new(
            new ApiGatewayResponseEnvelope<T>
            {
                StatusCode = statusCode,
                BodyContent = bodyContent,
                Headers = headers,
                IsBase64Encoded = isBase64Encoded,
            }
        );
}
