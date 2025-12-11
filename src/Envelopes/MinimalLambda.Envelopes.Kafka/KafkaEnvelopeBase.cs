using System.Text.Json.Serialization;
using Amazon.Lambda.KafkaEvents;
using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.Kafka;

/// <inheritdoc cref="KafkaEvent" />
/// <remarks>
///     This abstract class extends <see cref="KafkaEvent" /> and provides a foundation for
///     strongly typed Kafka record handling. Derived classes implement <see cref="ExtractPayload" />
///     to deserialize the Kafka record values into strongly typed
///     <see cref="KafkaEventRecordEnvelope" /> records using their chosen deserialization strategy.
/// </remarks>
public abstract class KafkaEnvelopeBase<T> : KafkaEvent, IRequestEnvelope
{
    /// <inheritdoc cref="KafkaEvent.Records" />
    public new required IDictionary<string, IList<KafkaEventRecordEnvelope>> Records { get; set; }

    /// <inheritdoc />
    public abstract void ExtractPayload(EnvelopeOptions options);

    /// <inheritdoc cref="KafkaEvent.KafkaEventRecord" />
    public class KafkaEventRecordEnvelope : KafkaEventRecord
    {
        /// <summary>Gets and sets the deserialized Kafka record value content</summary>
        [JsonIgnore]
        public T? ValueContent { get; set; }
    }
}
