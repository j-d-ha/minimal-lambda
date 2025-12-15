using System.Text.Json.Serialization;
using Amazon.Lambda.ApplicationLoadBalancerEvents;
using MinimalLambda.Envelopes.ApiGateway;
using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.Alb;

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

    public AlbResult Configure(Action<AlbResult> customizer)
    {
        customizer(this);
        return this;
    }

    public static AlbResult Create<T>(
        int statusCode,
        T? bodyContent,
        string? body,
        IDictionary<string, string>? headers,
        bool isBase64Encoded
    ) =>
        new(
            new AlbResponseEnvelope<T>
            {
                StatusCode = statusCode,
                BodyContent = bodyContent,
                Body = body ?? string.Empty,
                Headers = headers,
                IsBase64Encoded = isBase64Encoded,
            }
        );
}
