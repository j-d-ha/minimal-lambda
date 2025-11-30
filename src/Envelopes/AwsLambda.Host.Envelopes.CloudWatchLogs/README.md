# AwsLambda.Host.Envelopes.CloudWatchLogs

Strongly-typed CloudWatch Logs event handling for the AwsLambda.Host framework.

## Overview

This package provides envelope classes that extend the base [
`CloudWatchLogsEvent`](https://github.com/aws/aws-lambda-dotnet/tree/master/Libraries/src/Amazon.Lambda.CloudWatchLogsEvents)
with automatic base64 decoding, decompression, and deserialization of CloudWatch Logs data.
Instead of manually decoding and decompressing data from `Awslogs.Data`, you access the
deserialized payload directly via `envelope.AwslogsContent`.

| Envelope Class              | Base Class            | Use Case                                            |
|-----------------------------|-----------------------|-----------------------------------------------------|
| `CloudWatchLogsEnvelope`    | `CloudWatchLogsEvent` | CloudWatch Logs events with plain string log data   |
| `CloudWatchLogsEnvelope<T>` | `CloudWatchLogsEvent` | CloudWatch Logs events with typed deserialized data |

## Quick Start

Define your log data type and handler:

```csharp
using System.Text.Json;
using AwsLambda.Host.Builder;
using AwsLambda.Host.Envelopes.CloudWatchLogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = LambdaApplication.CreateBuilder();

builder.Services.ConfigureEnvelopeOptions(options =>
{
    options.JsonOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

var lambda = builder.Build();

// CloudWatchLogsEnvelope<T> provides access to the CloudWatch Logs event with each log message
// deserialized into type T
lambda.MapHandler(
    ([Event] CloudWatchLogsEnvelope<Log> logs, ILogger<Program> logger) =>
    {
        foreach (var logEvent in logs.AwslogsContent!.LogEvents)
        {
            logger.LogInformation("Log level: {Level}", logEvent.MessageContent?.Level);
            logger.LogInformation("Log message: {Message}", logEvent.MessageContent?.Message);
            logger.LogInformation("Request ID: {RequestId}", logEvent.MessageContent?.RequestId);
        }
    }
);

await lambda.RunAsync();

public record Log(string Level, string Message, string RequestId);
```

> [!TIP]
> If your log messages are plain strings (not JSON data), use `CloudWatchLogsEnvelope` instead of
> `CloudWatchLogsEnvelope<T>` to avoid deserialization errors. `CloudWatchLogsEnvelope` sets each
> `MessageContent` to the raw string message without attempting deserialization.

## Custom Envelopes

To implement custom deserialization logic, extend `CloudWatchLogsEnvelopeBase<T>`, override the
`ExtractPayload` method, and call `base.ExtractPayload(options)` to deserialize the CloudWatch Logs
envelope structure, then deserialize each log message:

```csharp
// Example: Custom XML deserialization
public sealed class CloudWatchLogsXmlEnvelope<T> : CloudWatchLogsEnvelopeBase<T>
{
    private static readonly XmlSerializer Serializer = new(typeof(T));

    public override void ExtractPayload(EnvelopeOptions options)
    {
        base.ExtractPayload(options);

        foreach (var logEvent in AwslogsContent!.LogEvents)
        {
            using var stringReader = new StringReader(logEvent.Message);
            using var xmlReader = XmlReader.Create(stringReader, options.XmlReaderSettings);
            logEvent.MessageContent = (T)Serializer.Deserialize(xmlReader)!;
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
[JsonSerializable(typeof(CloudWatchLogsEnvelope<LogData>))]
[JsonSerializable(typeof(LogData))]
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
