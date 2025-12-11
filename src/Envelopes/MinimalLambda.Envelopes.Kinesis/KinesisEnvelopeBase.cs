using System.Text.Json.Serialization;
using Amazon.Lambda.KinesisEvents;
using AwsLambda.Host.Envelopes;
using AwsLambda.Host.Options;

namespace MinimalLambda.Envelopes.Kinesis;

/// <inheritdoc cref="KinesisEvent" />
/// <remarks>
///     This abstract class extends <see cref="KinesisEvent" /> and provides a foundation for
///     strongly typed Kinesis record handling. Derived classes implement <see cref="ExtractPayload" />
///     to deserialize the Kinesis data into strongly typed <see cref="KinesisEventRecordEnvelope" />
///     records using their chosen deserialization strategy.
/// </remarks>
public abstract class KinesisEnvelopeBase<T> : KinesisEvent, IRequestEnvelope
{
    /// <inheritdoc cref="KinesisEvent.Records" />
    public new required IList<KinesisEventRecordEnvelope> Records { get; set; }

    /// <inheritdoc />
    public abstract void ExtractPayload(EnvelopeOptions options);

    /// <inheritdoc cref="KinesisEvent.KinesisEventRecord" />
    public class KinesisEventRecordEnvelope : KinesisEventRecord
    {
        /// <inheritdoc cref="KinesisEvent.KinesisEventRecord.Kinesis" />
        public new required RecordEnvelope Kinesis { get; set; }
    }

    /// <inheritdoc cref="KinesisEvent.Record" />
    public class RecordEnvelope : Record
    {
        /// <summary>
        ///     Gets and sets the deserialized <see cref="Amazon.Kinesis.Model.Record.Data" /> stream
        ///     content
        /// </summary>
        [JsonIgnore]
        public T? DataContent { get; set; }
    }
}
