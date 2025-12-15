using System.Text.Json.Serialization;
using Amazon.Lambda.ApplicationLoadBalancerEvents;
using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.Alb;

/// <summary>
///     Represents an HTTP response for AWS Lambda functions invoked by an Application Load
///     Balancer (ALB).
/// </summary>
/// <remarks>
///     This class wraps <see cref="ApplicationLoadBalancerResponse" /> and provides support for
///     response envelope customization through <see cref="IHttpResult{TSelf}" />. Use this type when
///     returning responses from Lambda functions triggered by ALB target groups.
/// </remarks>
public sealed class AlbResult : ApplicationLoadBalancerResponse, IHttpResult<AlbResult>
{
    [JsonIgnore]
    private readonly IResponseEnvelope? _inner;

    private AlbResult(ApplicationLoadBalancerResponse response)
    {
        _inner = response as IResponseEnvelope;
        StatusCode = response.StatusCode;
        StatusDescription = response.StatusDescription;
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
        Body = ((ApplicationLoadBalancerResponse)_inner).Body;
    }

    /// <inheritdoc />
    public static AlbResult Create<T>(
        int statusCode,
        T? bodyContent,
        IDictionary<string, string> headers,
        bool isBase64Encoded
    ) =>
        new(
            new AlbResponseEnvelope<T>
            {
                StatusCode = statusCode,
                BodyContent = bodyContent,
                Headers = headers,
                IsBase64Encoded = isBase64Encoded,
            }
        );
}
