using System.Text.Json;
using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.ApiGateway;

/// <inheritdoc cref="ApiGatewayV2RequestEnvelopeBase{T}" />
/// <remarks>
///     Provides the default implementation for deserializing request payloads using
///     <see cref="System.Text.Json.JsonSerializer" /> with the configured
///     <see cref="EnvelopeOptions.JsonOptions" />.
/// </remarks>
public sealed class ApiGatewayV2RequestEnvelope<T> : ApiGatewayV2RequestEnvelopeBase<T>
{
    /// <inheritdoc cref="IRequestEnvelope" />
    /// <remarks>This implementation deserializes the request body from JSON.</remarks>
    public override void ExtractPayload(EnvelopeOptions options) =>
        BodyContent = JsonSerializer.Deserialize<T>(Body, options.JsonOptions);
}
