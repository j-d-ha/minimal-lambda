using System.Text.Json.Serialization;
using Amazon.Lambda.SNSEvents;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.Sns;

/// <inheritdoc cref="SNSEvent" />
/// <remarks>
///     This abstract class extends <see cref="SNSEvent" /> and provides a foundation for strongly
///     typed SNS message handling. Derived classes implement <see cref="ExtractPayload" /> to
///     deserialize the message bodies into strongly typed <see cref="SnsRecordEnvelope" /> records
///     using their chosen deserialization strategy.
/// </remarks>
public abstract class SnsEnvelopeBase<T> : SNSEvent, IRequestEnvelope
{
    /// <inheritdoc cref="SNSEvent.Records" />
    public new required List<SnsRecordEnvelope> Records { get; set; }

    /// <inheritdoc />
    public abstract void ExtractPayload(EnvelopeOptions options);

    /// <inheritdoc cref="SNSEvent.SNSRecord" />
    public class SnsRecordEnvelope : SNSRecord
    {
        /// <inheritdoc cref="SNSEvent.SNSRecord.Sns" />
        public new required SnsMessageEnvelope Sns { get; set; }
    }

    /// <inheritdoc cref="SNSEvent.SNSMessage" />
    public class SnsMessageEnvelope : SNSMessage
    {
        /// <summary>Gets and sets the deserialized <see cref="SNSEvent.SNSMessage.Message" /> message body</summary>
        [JsonIgnore]
        public T? MessageContent { get; set; }
    }
}
