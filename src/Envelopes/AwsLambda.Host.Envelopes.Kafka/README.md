# AwsLambda.Host.Envelopes.Kafka

Strongly-typed Kafka event handling for the AwsLambda.Host framework.

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
using AwsLambda.Host.Builder;
using AwsLambda.Host.Envelopes.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

// KafkaEnvelope<OrderEvent> provides access to the Kafka event and deserialized OrderEvent payloads
lambda.MapHandler(
    ([Event] KafkaEnvelope<OrderEvent> envelope, ILogger<Program> logger) =>
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

> **Note:** The context must be registered in both places because the Lambda event and payload are
> deserialized at different steps: the Lambda serializer deserializes the raw Kafka event, and the
> envelope options deserialize the base64-encoded message values into your payload type.

See the [AwsLambda.Host documentation](https://github.com/j-d-ha/aws-lambda-host) for more details
on AOT support.

## Related Packages

| Package                                                                                               | NuGet                                                                                                                                                            | Downloads                                                                                                                                                              |
|-------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [**AwsLambda.Host.Envelopes.Sns**](../AwsLambda.Host.Envelopes.Sns/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns/)                         |
| [**AwsLambda.Host.Envelopes.Sqs**](../AwsLambda.Host.Envelopes.Sqs/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs/)                         |
| [**AwsLambda.Host.Envelopes.Kinesis**](../AwsLambda.Host.Envelopes.Kinesis/README.md)                 | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kinesis)                 | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kinesis/)                 |
| [**AwsLambda.Host.Envelopes.KinesisFirehose**](../AwsLambda.Host.Envelopes.KinesisFirehose/README.md) | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.KinesisFirehose) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.KinesisFirehose/) |
| [**AwsLambda.Host.Envelopes.ApiGateway**](../AwsLambda.Host.Envelopes.ApiGateway/README.md)           | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway)           | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway/)           |
| [**AwsLambda.Host.Envelopes.Alb**](../AwsLambda.Host.Envelopes.Alb/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb/)                         |

## License

This project is licensed under the MIT License. See [LICENSE](../../LICENSE) for details.
