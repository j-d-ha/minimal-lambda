# MinimalLambda.Envelopes.Kafka

Strongly-typed Kafka event handling for the MinimalLambda framework.

## Overview

This package provides `KafkaEnvelope<T>`, which extends the base [
`KafkaEvent`](https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.KafkaEvents/README.md)
class with a generic `Records` collection that deserializes base64-encoded Kafka message values into
strongly-typed objects. Instead of manually decoding and parsing base64 data from `record.Value`,
you
access deserialized payloads directly via `record.ValueContent`.

| Envelope Class     | Base Class   | Use Case                                     |
|--------------------|--------------|----------------------------------------------|
| `KafkaEnvelope<T>` | `KafkaEvent` | Kafka event with deserialized message values |

## Quick Start

Define your message type and handler:

```csharp
using Amazon.Lambda.KafkaEvents;
using MinimalLambda.Builder;
using MinimalLambda.Envelopes.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

// KafkaEnvelope<OrderEvent> provides access to the Kafka event and deserialized OrderEvent payloads
lambda.MapHandler(
    ([FromEvent] KafkaEnvelope<OrderEvent> envelope, ILogger<Program> logger) =>
    {
        foreach (var topic in envelope.Records)
        {
            logger.LogInformation("Processing {Count} records from topic: {Topic}", topic.Value.Count, topic.Key);

            foreach (var record in topic.Value)
            {
                logger.LogInformation(
                    "Order ID: {OrderId}, Amount: {Amount}",
                    record.ValueContent?.OrderId,
                    record.ValueContent?.Amount
                );
            }
        }
    }
);

await lambda.RunAsync();

// Your message payload - will be deserialized from base64-encoded Kafka message value
internal record OrderEvent(string OrderId, decimal Amount, DateTime Timestamp);
```

## Custom Envelopes

To implement custom deserialization logic, extend `KafkaEnvelopeBase<T>` and override the
`ExtractPayload` method:

```csharp
// Example: Custom XML deserialization
public sealed class KafkaXmlEnvelope<T> : KafkaEnvelopeBase<T>
{
    private static readonly XmlSerializer Serializer = new(typeof(T));

    public override void ExtractPayload(EnvelopeOptions options)
    {
        foreach (var topic in Records)
        {
            foreach (var record in topic.Value)
            {
                using var reader = new StreamReader(
                    record.Value,
                    Encoding.UTF8,
                    leaveOpen: true
                );
                var base64String = reader.ReadToEnd();
                var xmlBytes = Convert.FromBase64String(base64String);
                using var xmlReader = XmlReader.Create(
                    new MemoryStream(xmlBytes),
                    options.XmlReaderSettings
                );
                record.ValueContent = (T)Serializer.Deserialize(xmlReader)!;
            }
        }
    }
}
```

This pattern allows you to support multiple serialization formats while maintaining the same
envelope interface.

## AOT Support

When using .NET Native AOT, register both the envelope and payload types in your
`JsonSerializerContext`:

```csharp
[JsonSerializable(typeof(KafkaEnvelope<OrderEvent>))]
[JsonSerializable(typeof(OrderEvent))]
internal partial class SerializerContext : JsonSerializerContext;
```

Register the serializer and configure envelope options to use the context:

```csharp
builder.Services.AddLambdaSerializerWithContext<SerializerContext>();

builder.Services.ConfigureEnvelopeOptions(options =>
{
    options.JsonOptions.TypeInfoResolver = SerializerContext.Default;
});
```

> [!NOTE]
> The context must be registered as the type resolver for both the envelope options and the Lambda
> serializer because the Lambda event and envelope payload are deserialized at different steps: the
> Lambda serializer deserializes the raw event, and the envelope options deserialize the envelope
> content into your payload type.

## Other Packages

Additional packages in the minimal-lambda framework for abstractions, observability, and event
source handling.

| Package                                                                                             | NuGet                                                                                                                                                          | Downloads                                                                                                                                                            |
|-----------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [**MinimalLambda**](../../MinimalLambda/README.md)                                                  | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.svg)](https://www.nuget.org/packages/MinimalLambda)                                                     | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.svg)](https://www.nuget.org/packages/MinimalLambda/)                                                     |
| [**MinimalLambda.Abstractions**](../../MinimalLambda.Abstractions/README.md)                        | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Abstractions.svg)](https://www.nuget.org/packages/MinimalLambda.Abstractions)                           | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Abstractions.svg)](https://www.nuget.org/packages/MinimalLambda.Abstractions/)                           |
| [**MinimalLambda.OpenTelemetry**](../../MinimalLambda.OpenTelemetry/README.md)                      | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.OpenTelemetry.svg)](https://www.nuget.org/packages/MinimalLambda.OpenTelemetry)                         | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.OpenTelemetry.svg)](https://www.nuget.org/packages/MinimalLambda.OpenTelemetry/)                         |
| [**MinimalLambda.Envelopes**](../MinimalLambda.Envelopes/README.md)                                 | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Envelopes.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes)                                 | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Envelopes.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes/)                                 |
| [**MinimalLambda.Envelopes.Sqs**](../MinimalLambda.Envelopes.Sqs/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Envelopes.Sqs.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Sqs)                         | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Envelopes.Sqs.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Sqs/)                         |
| [**MinimalLambda.Envelopes.ApiGateway**](../MinimalLambda.Envelopes.ApiGateway/README.md)           | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.ApiGateway)           | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.ApiGateway/)           |
| [**MinimalLambda.Envelopes.Sns**](../MinimalLambda.Envelopes.Sns/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Envelopes.Sns.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Sns)                         | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Envelopes.Sns.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Sns/)                         |
| [**MinimalLambda.Envelopes.Kinesis**](../MinimalLambda.Envelopes.Kinesis/README.md)                 | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Kinesis)                 | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Kinesis/)                 |
| [**MinimalLambda.Envelopes.KinesisFirehose**](../MinimalLambda.Envelopes.KinesisFirehose/README.md) | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.KinesisFirehose) | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.KinesisFirehose/) |
| [**MinimalLambda.Envelopes.Kafka**](../MinimalLambda.Envelopes.Kafka/README.md)                     | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Envelopes.Kafka.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Kafka)                     | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Envelopes.Kafka.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Kafka/)                     |
| [**MinimalLambda.Envelopes.CloudWatchLogs**](../MinimalLambda.Envelopes.CloudWatchLogs/README.md)   | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Envelopes.CloudWatchLogs.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.CloudWatchLogs)   | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Envelopes.CloudWatchLogs.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.CloudWatchLogs/)   |
| [**MinimalLambda.Envelopes.Alb**](../MinimalLambda.Envelopes.Alb/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Envelopes.Alb.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Alb)                         | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Envelopes.Alb.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Alb/)                         |

## License

This project is licensed under the MIT License. See [LICENSE](../../LICENSE) for details.
