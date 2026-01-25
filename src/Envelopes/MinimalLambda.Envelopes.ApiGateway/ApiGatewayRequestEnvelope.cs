using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.ApiGateway;

/// <inheritdoc cref="ApiGatewayRequestEnvelopeBase{T}" />
/// <remarks>
///     Provides the default implementation for deserializing request payloads using
///     <see cref="System.Text.Json.JsonSerializer" /> with the configured
///     <see cref="EnvelopeOptions.JsonOptions" />.
/// </remarks>
public sealed class ApiGatewayRequestEnvelope<T> : ApiGatewayRequestEnvelopeBase<T>
{
    /// <inheritdoc cref="IRequestEnvelope" />
    /// <remarks>This implementation deserializes the request body from JSON.</remarks>
    [UnconditionalSuppressMessage(
        "Aot",
        "IL3050:RequiresDynamicCode",
        Justification =
            "Safe when EnvelopeOptions.JsonOptions includes source-generated context for T")]
    [UnconditionalSuppressMessage(
        "Aot",
        "IL2026:RequiresUnreferencedCode",
        Justification =
            "Safe when EnvelopeOptions.JsonOptions includes source-generated context for T")]
    public override void ExtractPayload(EnvelopeOptions options) =>
        BodyContent = JsonSerializer.Deserialize<T>(Body, options.JsonOptions);
}
