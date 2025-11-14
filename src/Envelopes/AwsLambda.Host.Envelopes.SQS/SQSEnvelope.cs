using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.SQSEvents;

namespace AwsLambda.Host.Envelopes.SQS;

/// <inheritdoc cref="SQSEvent" />
public class SQSEnvelope<T> : SQSEvent, IEnvelope
{
    /// <summary>Get and sets the Records</summary>
    public new required List<SQSMessageEnvelope> Records { get; set; }

    public void ExtractPayload(JsonSerializerOptions options)
    {
        foreach (var record in Records)
            record.Body = JsonSerializer.Deserialize<T>(((SQSMessage)record).Body, options);
    }

    public void PackPayload(JsonSerializerOptions options)
    {
        foreach (var record in Records)
            ((SQSMessage)record).Body = JsonSerializer.Serialize(record.Body, options);
    }

    /// <inheritdoc />
    public class SQSMessageEnvelope : SQSMessage
    {
        /// <summary>Get and sets the Body</summary>
        [JsonIgnore]
        public new required T? Body { get; set; }
    }
}
