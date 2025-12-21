using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using MinimalLambda.Options;

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
    [UnconditionalSuppressMessage(
        "Aot",
        "IL3050:RequiresDynamicCode",
        Justification = "Safe when EnvelopeOptions.JsonOptions includes source-generated context for T"
    )]
    [UnconditionalSuppressMessage(
        "Aot",
        "IL2026:RequiresUnreferencedCode",
        Justification = "Safe when EnvelopeOptions.JsonOptions includes source-generated context for T"
    )]
    public override void PackPayload(EnvelopeOptions options) =>
        Body = JsonSerializer.Serialize(BodyContent, options.JsonOptions);
}
