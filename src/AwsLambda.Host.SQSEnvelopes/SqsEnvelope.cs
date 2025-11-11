using System.Text.Json.Serialization;
using Amazon.Lambda.SQSEvents;

namespace AwsLambda.Host.SQSEnvelopes;

/// <inheritdoc cref="SQSEvent" />
public class SqsEnvelope<T> : SQSEvent, IJsonSerializable
{
    /// <summary>Get and sets the Records</summary>
    public new required List<SqsMessageEnvelope> Records { get; set; }

    public static void RegisterTypeInfo(IList<JsonConverter> converters) =>
        converters.Add(new SqsEnvelopeJsonConverter<SqsEnvelope<T>>());

    public class SqsMessageEnvelope : SQSMessage
    {
        public new required T Body { get; set; }
    }
}
