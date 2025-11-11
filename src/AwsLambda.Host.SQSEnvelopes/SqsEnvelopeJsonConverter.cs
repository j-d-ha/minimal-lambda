using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.SQSEvents;

namespace AwsLambda.Host.SQSEnvelopes;

public class SqsEnvelopeJsonConverter<T> : JsonConverter<SqsEnvelope<T>>
{
    public override SqsEnvelope<T>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        // Deserialize to the base SQSEvent first
        var baseEvent = JsonSerializer.Deserialize<SQSEvent>(ref reader, options);
        if (baseEvent == null)
            return null;

        // Convert records to SqsMessage<T>
        var records = new List<SqsEnvelope<T>.SqsMessageEnvelope>(baseEvent.Records.Count);

        foreach (var record in baseEvent.Records)
        {
            var body = JsonSerializer.Deserialize<T>(record.Body, options);

            var recordT = new SqsEnvelope<T>.SqsMessageEnvelope { Body = body };

            records.Add(recordT);
        }

        return new SqsEnvelope<T> { Records = records };
    }

    public override void Write(
        Utf8JsonWriter writer,
        SqsEnvelope<T> value,
        JsonSerializerOptions options
    )
    {
        var records = new List<SQSEvent.SQSMessage>(value.Records.Count);

        foreach (var record in value.Records)
        {
            var body = JsonSerializer.Serialize(record.Body, options);
            SQSEvent.SQSMessage outRecord = record;
            outRecord.Body = body;
            records.Add(outRecord);
        }

        var outSqsEvent = new SQSEvent { Records = records };
        JsonSerializer.Serialize(writer, outSqsEvent, options);
    }
}
