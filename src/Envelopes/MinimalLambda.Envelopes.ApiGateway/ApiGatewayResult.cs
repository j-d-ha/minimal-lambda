using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using AwsLambda.Host.Options;
using Microsoft.AspNetCore.Http;

namespace AwsLambda.Host.Envelopes.ApiGateway;

public sealed class ApiGatewayResult : APIGatewayProxyResponse, IResponseEnvelope
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

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                     Headers Helpers                      │
    //      └──────────────────────────────────────────────────────────┘

    public ApiGatewayResult AddHeader(string key, string value)
    {
        Headers[key] = value;

        return this;
    }

    public ApiGatewayResult AddContentType(string contentType) =>
        AddHeader("Content-Type", contentType);

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                      Basic Fatories                      │
    //      └──────────────────────────────────────────────────────────┘

    public static ApiGatewayResult Create(APIGatewayProxyResponse response) => new(response);

    public static ApiGatewayResult Create<T>(ApiGatewayResponseEnvelopeBase<T> response) =>
        new(response);

    public static ApiGatewayResult Create<T>(
        int statusCode,
        T? bodyContent,
        Dictionary<string, string> headers,
        IDictionary<string, IList<string>> multiValueHeaders
    ) =>
        Create(
            new ApiGatewayResponseEnvelope<T>
            {
                BodyContent = bodyContent,
                StatusCode = statusCode,
                Headers = headers,
                MultiValueHeaders = multiValueHeaders,
            }
        );

    public static ApiGatewayResult Create<T>(
        int statusCode,
        string body,
        Dictionary<string, string> headers,
        IDictionary<string, IList<string>> multiValueHeaders
    ) =>
        Create(
            new APIGatewayProxyResponse
            {
                StatusCode = statusCode,
                Headers = headers,
                MultiValueHeaders = multiValueHeaders,
                Body = body,
            }
        );

    public static ApiGatewayResult Json<T>(int statusCode, T bodyContent) =>
        Create(
            new ApiGatewayResponseEnvelope<T>
            {
                BodyContent = bodyContent,
                StatusCode = statusCode,
                Headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = "application/json; charset=utf-8",
                },
            }
        );

    public static ApiGatewayResult Text(int statusCode, string body) =>
        Create(
            new APIGatewayProxyResponse
            {
                StatusCode = statusCode,
                Headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = "text/plain; charset=utf-8",
                },
                Body = body,
            }
        );

    public static ApiGatewayResult Status(int statusCode) =>
        Create(new APIGatewayProxyResponse { StatusCode = statusCode });

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                  Status Code Factories                   │
    //      └──────────────────────────────────────────────────────────┘

    public static ApiGatewayResult Ok() => Status(StatusCodes.Status200OK);

    public static ApiGatewayResult Ok<T>(T bodyContent) =>
        Json(StatusCodes.Status200OK, bodyContent);

    public static ApiGatewayResult BadRequest() => Status(StatusCodes.Status400BadRequest);

    public static ApiGatewayResult BadRequest<T>(T bodyContent) =>
        Json(StatusCodes.Status400BadRequest, bodyContent);

    public static ApiGatewayResult InternalServerError() =>
        Status(StatusCodes.Status500InternalServerError);

    public static ApiGatewayResult InternalServerError<T>(T bodyContent) =>
        Json(StatusCodes.Status500InternalServerError, bodyContent);
}
