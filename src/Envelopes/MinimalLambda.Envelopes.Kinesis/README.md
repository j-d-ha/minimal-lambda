# AwsLambda.Host.Envelopes.Kinesis

Strongly-typed Kinesis event handling for the AwsLambda.Host framework.

## Overview

This package provides `KinesisEnvelope<T>`, which extends the base [
`KinesisEvent`](https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.KinesisEvents/README.md)
class with a generic `Records` collection that deserializes Kinesis data streams into strongly-typed
objects. Instead of manually decoding and parsing base64 data from `record.Kinesis.Data`, you access
deserialized payloads directly via `record.Kinesis.DataContent`.

| Envelope Class       | Base Class     | Use Case                                       |
|----------------------|----------------|------------------------------------------------|
| `KinesisEnvelope<T>` | `KinesisEvent` | Kinesis event with deserialized stream records |

## Quick Start

Define your record type and handler:

```csharp
using Amazon.Lambda.KinesisEvents;
using AwsLambda.Host.Builder;
using AwsLambda.Host.Envelopes.Kinesis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

// KinesisEnvelope<StreamRecord> provides access to the Kinesis event and deserialized StreamRecord payloads
lambda.MapHandler(
    ([Event] KinesisEnvelope<StreamRecord> envelope, ILogger<Program> logger) =>
    {
        foreach (var record in envelope.Records)
        {
            logger.LogInformation(
                "Stream Record: {EventName} at {Timestamp}",
                record.Kinesis.DataContent?.EventName,
                record.Kinesis.DataContent?.Timestamp
            );
        }
    }
);

await lambda.RunAsync();

// Your record payload - will be deserialized from base64-encoded Kinesis data stream
internal record StreamRecord(string EventName, DateTime Timestamp);
```

## Custom Envelopes

To implement custom deserialization logic, extend `KinesisEnvelopeBase<T>` and override the
`ExtractPayload` method:

```csharp
// Example: Custom XML deserialization
public sealed class KinesisXmlEnvelope<T> : KinesisEnvelopeBase<T>
{
    private static readonly XmlSerializer Serializer = new(typeof(T));

    public override void ExtractPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
        {
            using var reader = new StreamReader(
                record.Kinesis.Data,
                Encoding.UTF8,
                leaveOpen: true
            );
            var base64String = reader.ReadToEnd();
            var xmlBytes = Convert.FromBase64String(base64String);
            using var xmlReader = XmlReader.Create(
                new MemoryStream(xmlBytes),
                options.XmlReaderSettings
            );
            record.Kinesis.DataContent = (T)Serializer.Deserialize(xmlReader)!;
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
[JsonSerializable(typeof(KinesisEnvelope<StreamRecord>))]
[JsonSerializable(typeof(StreamRecord))]
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

Additional packages in the aws-lambda-host framework for abstractions, observability, and event
source handling.

| Package                                                                                               | NuGet                                                                                                                                                            | Downloads                                                                                                                                                              |
|-------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [**AwsLambda.Host**](../../AwsLambda.Host/README.md)                                                  | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.svg)](https://www.nuget.org/packages/AwsLambda.Host)                                                     | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.svg)](https://www.nuget.org/packages/AwsLambda.Host/)                                                     |
| [**AwsLambda.Host.Abstractions**](../../AwsLambda.Host.Abstractions/README.md)                        | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Abstractions.svg)](https://www.nuget.org/packages/AwsLambda.Host.Abstractions)                           | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Abstractions.svg)](https://www.nuget.org/packages/AwsLambda.Host.Abstractions/)                           |
| [**AwsLambda.Host.OpenTelemetry**](../../AwsLambda.Host.OpenTelemetry/README.md)                      | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.OpenTelemetry.svg)](https://www.nuget.org/packages/AwsLambda.Host.OpenTelemetry)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.OpenTelemetry.svg)](https://www.nuget.org/packages/AwsLambda.Host.OpenTelemetry/)                         |
| [**AwsLambda.Host.Envelopes.Sqs**](../AwsLambda.Host.Envelopes.Sqs/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs/)                         |
| [**AwsLambda.Host.Envelopes.ApiGateway**](../AwsLambda.Host.Envelopes.ApiGateway/README.md)           | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway)           | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway/)           |
| [**AwsLambda.Host.Envelopes.Sns**](../AwsLambda.Host.Envelopes.Sns/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns/)                         |
| [**AwsLambda.Host.Envelopes.Kinesis**](../AwsLambda.Host.Envelopes.Kinesis/README.md)                 | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kinesis)                 | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kinesis/)                 |
| [**AwsLambda.Host.Envelopes.KinesisFirehose**](../AwsLambda.Host.Envelopes.KinesisFirehose/README.md) | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.KinesisFirehose) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.KinesisFirehose/) |
| [**AwsLambda.Host.Envelopes.Kafka**](../AwsLambda.Host.Envelopes.Kafka/README.md)                     | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Kafka.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kafka)                     | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Kafka.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kafka/)                     |
| [**AwsLambda.Host.Envelopes.CloudWatchLogs**](../AwsLambda.Host.Envelopes.CloudWatchLogs/README.md)   | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.CloudWatchLogs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.CloudWatchLogs)   | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.CloudWatchLogs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.CloudWatchLogs/)   |
| [**AwsLambda.Host.Envelopes.Alb**](../AwsLambda.Host.Envelopes.Alb/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb/)                         |

## License

This project is licensed under the MIT License. See [LICENSE](../../LICENSE) for details.
