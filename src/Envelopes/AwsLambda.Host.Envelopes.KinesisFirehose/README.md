# AwsLambda.Host.Envelopes.KinesisFirehose

Strongly-typed Kinesis Firehose event handling for the AwsLambda.Host framework.

## Overview

This package provides strongly-typed envelopes for handling Kinesis Firehose transformation events
in Lambda functions. It contains classes that can be used as input and output types for Lambda
functions that transform data records for Kinesis Data Firehose.

The envelopes extend the base [
`KinesisFirehoseEvent`](https://github.com/aws/aws-lambda-dotnet/tree/master/Libraries/src/Amazon.Lambda.KinesisFirehoseEvents)
and [
`KinesisFirehoseResponse`](https://github.com/aws/aws-lambda-dotnet/tree/master/Libraries/src/Amazon.Lambda.KinesisFirehoseEvents)
with strongly-typed `DataContent` properties for easier data
transformation. Instead of manually decoding base64 data from `record.Data` and parsing JSON, you
access deserialized payloads directly via `record.DataContent`:

| Envelope Class                       | Base Class                | Use Case                                        |
|--------------------------------------|---------------------------|-------------------------------------------------|
| `KinesisFirehoseEventEnvelope<T>`    | `KinesisFirehoseEvent`    | Firehose events with deserialized data records  |
| `KinesisFirehoseResponseEnvelope<T>` | `KinesisFirehoseResponse` | Firehose responses with serialized data records |

## Quick Start

Define your data types, then create a transformation handler:

```csharp
using System;
using AwsLambda.Host.Builder;
using AwsLambda.Host.Envelopes.KinesisFirehose;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

// KinesisFirehoseEventEnvelope<InputData> provides the Firehose event with deserialized records
// KinesisFirehoseResponseEnvelope<OutputData> wraps the response and serializes the transformed data
lambda.MapHandler(
    ([Event] KinesisFirehoseEventEnvelope<InputData> request, ILogger<Program> logger) =>
    {
        var response = new KinesisFirehoseResponseEnvelope<OutputData>
        {
            Records = new List<KinesisFirehoseResponseEnvelope<OutputData>.FirehoseRecordEnvelope>()
        };

        foreach (var record in request.Records)
        {
            logger.LogInformation("Processing record: {Name}", record.DataContent?.Name);

            // Transform the data
            var transformedData = new OutputData(
                $"{record.DataContent?.Name?.ToUpper()}",
                record.DataContent?.Value * 2 ?? 0
            );

            // Add to response
            response.Records.Add(new KinesisFirehoseResponseEnvelope<OutputData>.FirehoseRecordEnvelope
            {
                RecordId = record.RecordId,
                Result = "Ok",
                DataContent = transformedData
            });
        }

        return response;
    }
);

await lambda.RunAsync();

// Your input and output data types
internal record InputData(string Name, int Value);

internal record OutputData(string TransformedName, int TransformedValue);
```

## Custom Envelopes

To implement custom deserialization or serialization logic, extend the appropriate base class and
override the payload handling method:

```csharp
// Example: Custom XML deserialization for Firehose events
public sealed class XmlKinesisFirehoseEventEnvelope<T> : KinesisFirehoseEventEnvelopeBase<T>
{
    public override void ExtractPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
        {
            var decodedData = record.DecodeData();
            using var stringReader = new StringReader(decodedData);
            using var xmlReader = XmlReader.Create(stringReader, options.XmlReaderSettings);
            var serializer = new XmlSerializer(typeof(T));
            record.DataContent = (T)serializer.Deserialize(xmlReader)!;
        }
    }
}

// Example: Custom XML serialization for Firehose responses
public sealed class XmlKinesisFirehoseResponseEnvelope<T> : KinesisFirehoseResponseEnvelopeBase<T>
{
    public override void PackPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
        {
            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, options.XmlWriterSettings);
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(xmlWriter, record.DataContent);
            record.EncodeData(stringWriter.ToString());
        }
    }
}
```

This pattern allows you to support multiple serialization formats while maintaining the same
envelope interface.

## AOT Support

When using .NET Native AOT, register all envelope and payload types in your `JsonSerializerContext`:

```csharp
[JsonSerializable(typeof(KinesisFirehoseEventEnvelope<InputData>))]
[JsonSerializable(typeof(KinesisFirehoseResponseEnvelope<OutputData>))]
[JsonSerializable(typeof(InputData))]
[JsonSerializable(typeof(OutputData))]
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
> deserialized at different steps: the Lambda serializer deserializes the Firehose event, and the
> envelope options deserialize the record data and serialize the response data.

See the [example project](../../examples/AwsLambda.Host.Example.Events/Program.cs) for a complete
working example.

## Related Packages

| Package                                                                                     | NuGet                                                                                                                                                  | Downloads                                                                                                                                                    |
|---------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [**AwsLambda.Host.Envelopes.Kinesis**](../AwsLambda.Host.Envelopes.Kinesis/README.md)       | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kinesis)       | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kinesis/)       |
| [**AwsLambda.Host.Envelopes.Kafka**](../AwsLambda.Host.Envelopes.Kafka/README.md)           | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Kafka.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kafka)           | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Kafka.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kafka/)           |
| [**AwsLambda.Host.Envelopes.Sns**](../AwsLambda.Host.Envelopes.Sns/README.md)               | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns)               | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns/)               |
| [**AwsLambda.Host.Envelopes.Sqs**](../AwsLambda.Host.Envelopes.Sqs/README.md)               | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs)               | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs/)               |
| [**AwsLambda.Host.Envelopes.ApiGateway**](../AwsLambda.Host.Envelopes.ApiGateway/README.md) | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway/) |
| [**AwsLambda.Host.Envelopes.Alb**](../AwsLambda.Host.Envelopes.Alb/README.md)               | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb)               | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb/)               |

## License

This project is licensed under the MIT License. See [LICENSE](../../LICENSE) for details.
