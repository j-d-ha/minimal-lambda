using System.Text.Json.Serialization;
using Amazon.Lambda.SQSEvents;

namespace AwsLambda.Host.Envelopes.SQS;

/// <inheritdoc cref="SQSEvent" />
public class SqsEnvelope<T> : SQSEvent, IJsonSerializable
{
    /// <summary>Get and sets the Records</summary>
    public new required List<SqsMessageEnvelope> Records { get; set; }

    /// <inheritdoc />
    public static void RegisterConverter(IList<JsonConverter> converters) =>
        converters.Add(new SqsEnvelopeJsonConverter<SqsEnvelope<T>>());

    /// <inheritdoc />
    public class SqsMessageEnvelope : SQSMessage
    {
        /// <summary>Get and sets the Body</summary>
        [JsonIgnore]
        public new required T? Body { get; set; }
    }
}
