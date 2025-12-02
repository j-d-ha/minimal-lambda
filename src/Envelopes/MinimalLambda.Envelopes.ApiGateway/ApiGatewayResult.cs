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
        base.StatusCode = response.StatusCode;
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

    public static ApiGatewayResult StatusCode(int statusCode) =>
        Create(new APIGatewayProxyResponse { StatusCode = statusCode });

    //      ┌──────────────────────────────────────────────────────────┐
    //      │                  StatusCode Code Factories               │
    //      └──────────────────────────────────────────────────────────┘

    // ── 200 Ok ───────────────────────────────────────────────────────────────────────

    public static ApiGatewayResult Ok() => StatusCode(StatusCodes.Status200OK);

    public static ApiGatewayResult Ok<T>(T bodyContent) =>
        Json(StatusCodes.Status200OK, bodyContent);

    // ── 201 No Content ───────────────────────────────────────────────────────────────

    public static ApiGatewayResult NoContent() => StatusCode(StatusCodes.Status204NoContent);

    // ── 401 Unauthorized ─────────────────────────────────────────────────────────────

    public static ApiGatewayResult Unauthorized() => StatusCode(StatusCodes.Status401Unauthorized);

    // ── 404 Not Found ────────────────────────────────────────────────────────────────

    public static ApiGatewayResult NotFound() => StatusCode(StatusCodes.Status404NotFound);

    public static ApiGatewayResult NotFound<T>(T bodyContent) =>
        Json(StatusCodes.Status404NotFound, bodyContent);

    // ── 404 Bad Request ──────────────────────────────────────────────────────────────

    public static ApiGatewayResult BadRequest() => StatusCode(StatusCodes.Status400BadRequest);

    public static ApiGatewayResult BadRequest<T>(T bodyContent) =>
        Json(StatusCodes.Status400BadRequest, bodyContent);

    // ── 409 Conflict ─────────────────────────────────────────────────────────────────

    public static ApiGatewayResult Conflict() =>
        StatusCode(StatusCodes.Status500InternalServerError);

    public static ApiGatewayResult Conflict<T>(T bodyContent) =>
        Json(StatusCodes.Status409Conflict, bodyContent);

    // ── 422 Unprocessable Entity ─────────────────────────────────────────────────────

    public static ApiGatewayResult UnprocessableEntity() =>
        StatusCode(StatusCodes.Status422UnprocessableEntity);

    public static ApiGatewayResult UnprocessableEntity<T>(T bodyContent) =>
        Json(StatusCodes.Status422UnprocessableEntity, bodyContent);

    // ── 500 Internal Server Error ────────────────────────────────────────────────────

    public static ApiGatewayResult InternalServerError() =>
        StatusCode(StatusCodes.Status500InternalServerError);

    public static ApiGatewayResult InternalServerError<T>(T bodyContent) =>
        Json(StatusCodes.Status500InternalServerError, bodyContent);
}
