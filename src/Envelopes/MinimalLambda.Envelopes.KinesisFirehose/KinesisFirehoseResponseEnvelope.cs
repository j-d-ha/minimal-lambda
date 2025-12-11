using System.Text.Json;
using AwsLambda.Host.Envelopes;
using AwsLambda.Host.Options;

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
    public override void PackPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
        {
            var serializedData = JsonSerializer.Serialize(record.DataContent, options.JsonOptions);
            record.EncodeData(serializedData);
        }
    }
}
