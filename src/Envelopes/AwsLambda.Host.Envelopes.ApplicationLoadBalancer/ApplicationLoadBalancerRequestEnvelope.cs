using System.Text.Json;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.ApplicationLoadBalancer;

/// <inheritdoc cref="AlbRequestEnvelopeBase{T}" />
/// <remarks>
///     Provides the default implementation for deserializing request payloads using
///     <see cref="System.Text.Json.JsonSerializer" /> with the configured
///     <see cref="EnvelopeOptions.JsonOptions" />.
/// </remarks>
public sealed class AlbRequestEnvelope<T> : AlbRequestEnvelopeBase<T>
{
    /// <inheritdoc cref="IRequestEnvelope" />
    /// <remarks>This implementation deserializes the request body from JSON.</remarks>
    public override void ExtractPayload(EnvelopeOptions options) =>
        BodyContent = JsonSerializer.Deserialize<T>(Body, options.JsonOptions);
}
