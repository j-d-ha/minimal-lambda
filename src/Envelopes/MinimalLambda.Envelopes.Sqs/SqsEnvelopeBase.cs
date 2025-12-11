using System.Text.Json.Serialization;
using Amazon.Lambda.SQSEvents;
using AwsLambda.Host.Envelopes;
using AwsLambda.Host.Options;

namespace MinimalLambda.Envelopes.Sqs;

/// <inheritdoc cref="SQSEvent" />
/// <remarks>
///     This abstract class extends <see cref="SQSEvent" /> and provides a foundation for strongly
///     typed SQS message handling. Derived classes implement <see cref="ExtractPayload" /> to
///     deserialize the message bodies into strongly typed <see cref="SqsMessageEnvelope" /> records
///     using their chosen deserialization strategy.
/// </remarks>
public abstract class SqsEnvelopeBase<T> : SQSEvent, IRequestEnvelope
{
    /// <inheritdoc cref="SQSEvent.Records" />
    public new required List<SqsMessageEnvelope> Records { get; set; }

    /// <inheritdoc />
    public abstract void ExtractPayload(EnvelopeOptions options);

    /// <inheritdoc cref="SQSEvent.SQSMessage" />
    public class SqsMessageEnvelope : SQSMessage
    {
        /// <summary>Get and sets the deserialized <see cref="SQSEvent.SQSMessage.Body" /></summary>
        [JsonIgnore]
        public T? BodyContent { get; set; }
    }
}
