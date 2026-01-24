using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.Kafka;

/// <inheritdoc cref="KafkaEnvelopeBase{T}" />
/// <remarks>
///     Provides the default implementation for deserializing Kafka record values using
///     <see cref="System.Text.Json.JsonSerializer" /> with the configured
///     <see cref="EnvelopeOptions.JsonOptions" />.
/// </remarks>
public sealed class KafkaEnvelope<T> : KafkaEnvelopeBase<T>
{
    /// <inheritdoc cref="IRequestEnvelope" />
    /// <remarks>This implementation deserializes each base64-encoded value stream from JSON.</remarks>
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
    public override void ExtractPayload(EnvelopeOptions options)
    {
        foreach (var topic in Records)
            foreach (var record in topic.Value)
            {
                using var reader = new StreamReader(record.Value, Encoding.UTF8, leaveOpen: true);
                var base64String = reader.ReadToEnd();
                var jsonBytes = Convert.FromBase64String(base64String);
                record.ValueContent = JsonSerializer.Deserialize<T>(jsonBytes, options.JsonOptions);
            }
    }
}
