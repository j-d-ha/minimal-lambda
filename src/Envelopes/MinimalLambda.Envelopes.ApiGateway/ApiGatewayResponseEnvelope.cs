using System.Text.Json;
using AwsLambda.Host.Envelopes;
using AwsLambda.Host.Options;

namespace MinimalLambda.Envelopes.ApiGateway;

/// <inheritdoc cref="ApiGatewayResponseEnvelopeBase{T}" />
/// <remarks>
///     Provides the default implementation for serializing response payloads using
///     <see cref="System.Text.Json.JsonSerializer" /> with the configured
///     <see cref="EnvelopeOptions.JsonOptions" />.
/// </remarks>
public sealed class ApiGatewayResponseEnvelope<T> : ApiGatewayResponseEnvelopeBase<T>
{
    /// <inheritdoc cref="IResponseEnvelope" />
    /// <remarks>This implementation serializes the response body to JSON.</remarks>
    public override void PackPayload(EnvelopeOptions options) =>
        Body = JsonSerializer.Serialize(BodyContent, options.JsonOptions);
}
