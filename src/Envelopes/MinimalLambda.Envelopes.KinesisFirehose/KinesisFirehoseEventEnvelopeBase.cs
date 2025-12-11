using System.Text.Json.Serialization;
using Amazon.Lambda.KinesisFirehoseEvents;
using AwsLambda.Host.Envelopes;
using AwsLambda.Host.Options;

namespace MinimalLambda.Envelopes.KinesisFirehose;

/// <inheritdoc cref="KinesisFirehoseEvent" />
/// <remarks>
///     This abstract class extends <see cref="KinesisFirehoseEvent" /> and provides a foundation
///     for strongly typed Kinesis Firehose record handling. Derived classes implement
///     <see cref="ExtractPayload" /> to deserialize the Firehose data into strongly typed
///     <see cref="FirehoseRecordEnvelope" /> records using their chosen deserialization strategy.
/// </remarks>
public abstract class KinesisFirehoseEventEnvelopeBase<T> : KinesisFirehoseEvent, IRequestEnvelope
{
    /// <inheritdoc cref="KinesisFirehoseEvent.Records" />
    public new required IList<FirehoseRecordEnvelope> Records { get; set; }

    /// <inheritdoc />
    public abstract void ExtractPayload(EnvelopeOptions options);

    /// <inheritdoc cref="KinesisFirehoseEvent.FirehoseRecord" />
    public class FirehoseRecordEnvelope : FirehoseRecord
    {
        /// <summary>
        ///     Gets and sets the deserialized
        ///     <see cref="KinesisFirehoseEvent.FirehoseRecord.Base64EncodedData" /> content
        /// </summary>
        [JsonIgnore]
        public T? DataContent { get; set; }
    }
}
