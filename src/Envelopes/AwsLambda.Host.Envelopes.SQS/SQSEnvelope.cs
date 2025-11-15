using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.SQSEvents;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.SQS;

/// <inheritdoc cref="SQSEvent" />
/// <remarks>
/// This class extends <see cref="SQSEvent"/> and adds strongly typed <see cref="SQSMessageEnvelope"/>
/// records for easier serialization and deserialization of SQS message payloads.
/// </remarks>
public class SQSEnvelope<T> : SQSEvent, IEnvelope
{
    /// <summary>Get and sets the Records</summary>
    public new required List<SQSMessageEnvelope> Records { get; set; }

    public void ExtractPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
            record.BodyContent = JsonSerializer.Deserialize<T>(record.Body, options.JsonOptions);
    }

    public void PackPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
            record.Body = JsonSerializer.Serialize(record.BodyContent, options.JsonOptions);
    }

    /// <inheritdoc />
    public class SQSMessageEnvelope : SQSMessage
    {
        /// <summary>Get and sets the deserialized <see cref="SQSMessage.Body" /></summary>
        [JsonIgnore]
        public T? BodyContent { get; set; }
    }
}
