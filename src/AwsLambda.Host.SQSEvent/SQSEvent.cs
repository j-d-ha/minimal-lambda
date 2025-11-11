using System.Text.Json;
using Amazon.Lambda.Core;

namespace AwsLambda.Host.SQSEvent;

/// <inheritdoc cref="Amazon.Lambda.SQSEvents.SQSEvent" />
public class SQSEvent<T> : Amazon.Lambda.SQSEvents.SQSEvent, ILambdaRequest<SQSEvent<T>>
{
    public new required List<SQSMessage<T>> Records { get; set; }

    public static SQSEvent<T> Deserialize(
        Stream requestStream,
        ILambdaSerializer lambdaSerializer,
        JsonSerializerOptions? jsonSerializerOptions
    )
    {
        var baseResponse = lambdaSerializer.Deserialize<Amazon.Lambda.SQSEvents.SQSEvent>(
            requestStream
        );

        var records = new List<SQSMessage<T>>(baseResponse.Records.Count);

        foreach (var record in baseResponse.Records)
        {
            var body = JsonSerializer.Deserialize<T>(record.Body, jsonSerializerOptions);

            var recordT = new SQSMessage<T>
            {
                MessageId = record.MessageId,
                ReceiptHandle = record.ReceiptHandle,
                Md5OfBody = record.Md5OfBody,
                Md5OfMessageAttributes = record.Md5OfMessageAttributes,
                EventSourceArn = record.EventSourceArn,
                EventSource = record.EventSource,
                AwsRegion = record.AwsRegion,
                Attributes = record.Attributes,
                MessageAttributes = record.MessageAttributes,
                Body = body,
            };

            records.Add(recordT);
        }

        return new SQSEvent<T> { Records = records };
    }

    public class SQSMessage<T> : SQSMessage
    {
        public new T? Body { get; set; }
    }
}
