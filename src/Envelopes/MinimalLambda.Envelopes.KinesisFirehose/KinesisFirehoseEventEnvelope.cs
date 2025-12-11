using System.Text.Json;
using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.KinesisFirehose;

/// <inheritdoc cref="KinesisFirehoseEventEnvelopeBase{T}" />
/// <remarks>
///     Provides the default implementation for deserializing Kinesis Firehose data payloads using
///     <see cref="System.Text.Json.JsonSerializer" /> with the configured
///     <see cref="EnvelopeOptions.JsonOptions" />.
/// </remarks>
public sealed class KinesisFirehoseEventEnvelope<T> : KinesisFirehoseEventEnvelopeBase<T>
{
    /// <inheritdoc cref="IRequestEnvelope" />
    /// <remarks>This implementation deserializes each base64-encoded Firehose data record from JSON.</remarks>
    public override void ExtractPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
            record.DataContent = JsonSerializer.Deserialize<T>(
                record.DecodeData(),
                options.JsonOptions
            );
    }
}
