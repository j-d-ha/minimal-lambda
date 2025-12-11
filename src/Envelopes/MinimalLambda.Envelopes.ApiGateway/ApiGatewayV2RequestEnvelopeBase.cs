using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.ApiGateway;

/// <inheritdoc cref="Amazon.Lambda.APIGatewayEvents.APIGatewayHttpApiV2ProxyRequest" />
/// <remarks>
///     This abstract class extends
///     <see cref="Amazon.Lambda.APIGatewayEvents.APIGatewayHttpApiV2ProxyRequest" /> and provides a
///     foundation for strongly typed request handling. Derived classes implement
///     <see cref="ExtractPayload" /> to deserialize the request body into a strongly typed
///     <see cref="BodyContent" /> property using their chosen deserialization strategy.
/// </remarks>
public abstract class ApiGatewayV2RequestEnvelopeBase<T>
    : APIGatewayHttpApiV2ProxyRequest,
        IRequestEnvelope
{
    /// <summary>The deserialized content of the <see cref="APIGatewayHttpApiV2ProxyRequest.Body" /></summary>
    [JsonIgnore]
    public T? BodyContent { get; set; }

    /// <inheritdoc />
    public abstract void ExtractPayload(EnvelopeOptions options);
}
