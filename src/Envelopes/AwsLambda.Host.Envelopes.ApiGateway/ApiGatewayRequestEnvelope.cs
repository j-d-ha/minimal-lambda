using System.Text.Json;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.ApiGateway;

/// <inheritdoc cref="ApiGatewayRequestEnvelopeBase{T}" />
/// <remarks>
///     Provides the default implementation for deserializing request payloads using
///     <see cref="System.Text.Json.JsonSerializer" /> with the configured
///     <see cref="EnvelopeOptions.JsonOptions" />.
/// </remarks>
public class ApiGatewayRequestEnvelope<T> : ApiGatewayRequestEnvelopeBase<T>
{
    /// <inheritdoc cref="IRequestEnvelope" />
    public override void ExtractPayload(EnvelopeOptions options) =>
        BodyContent = JsonSerializer.Deserialize<T>(Body, options.JsonOptions);
}
