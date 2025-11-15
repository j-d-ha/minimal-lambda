using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.SQSEvents;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.Sqs;

/// <inheritdoc cref="SQSEvent" />
/// <remarks>
///     This class extends <see cref="SQSEvent" /> and adds strongly typed
///     <see cref="SqsMessageEnvelope" /> records for easier serialization and deserialization of SQS
///     message payloads.
/// </remarks>
public class SqsEnvelope<T> : SQSEvent, IRequestEnvelope
{
    /// <inheritdoc cref="SQSEvent.Records" />
    public new required List<SqsMessageEnvelope> Records { get; set; }

    /// <inheritdoc />
    public void ExtractPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
            record.BodyContent = JsonSerializer.Deserialize<T>(record.Body, options.JsonOptions);
    }

    /// <inheritdoc />
    public class SqsMessageEnvelope : SQSMessage
    {
        /// <summary>Get and sets the deserialized <see cref="SQSEvent.SQSMessage.Body" /></summary>
        [JsonIgnore]
        public T? BodyContent { get; set; }
    }
}
