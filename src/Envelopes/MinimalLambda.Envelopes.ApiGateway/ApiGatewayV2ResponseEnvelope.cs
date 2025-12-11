using System.Text.Json;
using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.ApiGateway;

/// <inheritdoc cref="ApiGatewayV2ResponseEnvelopeBase{T}" />
/// <remarks>
///     Provides the default implementation for serializing response payloads using
///     <see cref="System.Text.Json.JsonSerializer" /> with the configured
///     <see cref="EnvelopeOptions.JsonOptions" />.
/// </remarks>
public sealed class ApiGatewayV2ResponseEnvelope<T> : ApiGatewayV2ResponseEnvelopeBase<T>
{
    /// <inheritdoc cref="IResponseEnvelope" />
    /// <remarks>This implementation serializes the response body to JSON.</remarks>
    public override void PackPayload(EnvelopeOptions options) =>
        Body = JsonSerializer.Serialize(BodyContent, options.JsonOptions);
}
