using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.SQSEvents;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.SQS;

/// <inheritdoc cref="SQSEvent" />
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
        /// <summary>Get and sets the BodyContent</summary>
        [JsonIgnore]
        public required T? BodyContent { get; set; }
    }
}
