using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.Kinesis;

/// <inheritdoc cref="KinesisEnvelopeBase{T}" />
/// <remarks>
///     Provides the default implementation for deserializing Kinesis data payloads using
///     <see cref="System.Text.Json.JsonSerializer" /> with the configured
///     <see cref="EnvelopeOptions.JsonOptions" />.
/// </remarks>
public sealed class KinesisEnvelope<T> : KinesisEnvelopeBase<T>
{
    /// <inheritdoc cref="IRequestEnvelope" />
    /// <remarks>This implementation deserializes each base64-encoded data stream from JSON.</remarks>
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
        foreach (var record in Records)
        {
            using var reader = new StreamReader(
                record.Kinesis.Data,
                Encoding.UTF8,
                leaveOpen: true);
            var base64String = reader.ReadToEnd();
            var jsonBytes = Convert.FromBase64String(base64String);
            record.Kinesis.DataContent = JsonSerializer.Deserialize<T>(
                jsonBytes,
                options.JsonOptions);
        }
    }
}
