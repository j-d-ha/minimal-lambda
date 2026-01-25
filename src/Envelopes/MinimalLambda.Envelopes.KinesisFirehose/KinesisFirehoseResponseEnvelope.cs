using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.KinesisFirehose;

/// <inheritdoc cref="KinesisFirehoseResponseEnvelopeBase{T}" />
/// <remarks>
///     Provides the default implementation for serializing Kinesis Firehose response payloads
///     using <see cref="System.Text.Json.JsonSerializer" /> with the configured
///     <see cref="EnvelopeOptions.JsonOptions" />.
/// </remarks>
public sealed class KinesisFirehoseResponseEnvelope<T> : KinesisFirehoseResponseEnvelopeBase<T>
{
    /// <inheritdoc cref="IResponseEnvelope" />
    /// <remarks>This implementation serializes each data content to JSON and encodes it as base64.</remarks>
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
    public override void PackPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
        {
            var serializedData = JsonSerializer.Serialize(record.DataContent, options.JsonOptions);
            record.EncodeData(serializedData);
        }
    }
}
