# AwsLambda.Host.Envelopes.Sqs

[![Main Build](https://github.com/j-d-ha/aws-lambda-host/actions/workflows/main-build.yaml/badge.svg)](https://github.com/j-d-ha/aws-lambda-host/actions/workflows/main-build.yaml)
[![codecov](https://codecov.io/gh/j-d-ha/aws-lambda-host/graph/badge.svg?token=BWORPTQ0UK)](https://codecov.io/gh/j-d-ha/aws-lambda-host)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=j-d-ha_aws-lambda-host&metric=alert_status&token=9fb519975d91379dcfbc6c13a4bd4207131af6e3)](https://sonarcloud.io/summary/new_code?id=j-d-ha_aws-lambda-host)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Strongly-typed SQS event handling for the AwsLambda.Host framework.

## Overview

This package provides `SqsEnvelope<T>`, which extends the base [
`SQSEvent`](https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.SQSEvents/README.md)
class with a generic `Records` collection that deserializes message bodies into strongly-typed
objects. Instead of manually parsing JSON from `record.Body`, you access deserialized payloads
directly via `record.BodyContent`.

| Envelope Class   | Base Class | Use Case                                   |
|------------------|------------|--------------------------------------------|
| `SqsEnvelope<T>` | `SQSEvent` | SQS event with deserialized message bodies |

## Quick Start

Define your message type and handler:

```csharp
using AwsLambda.Host;
using AwsLambda.Host.Envelopes.Sqs;

// Your message payload - will be deserialized from SQS message body
record Message(string Name);

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

// SqsEnvelope<Message> provides access to the SQS event and deserialized Message payloads
lambda.MapHandler(([Event] SqsEnvelope<Message> envelope, ILogger<Program> logger) =>
{
    foreach (var record in envelope.Records)
    {
        logger.LogInformation("Message: {Name}", record.BodyContent?.Name);
    }
});

await lambda.RunAsync();

record Message(string Name);
```

## AOT Support

When using .NET Native AOT, register both the envelope and payload types in your
`JsonSerializerContext`:

```csharp
[JsonSerializable(typeof(SqsEnvelope<Message>))]
[JsonSerializable(typeof(Message))]
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
> deserialized at different steps: the Lambda serializer deserializes the raw SQS event, and the
> envelope options deserialize the message bodies into your payload type.

See the [example project](../../examples/AwsLambda.Host.Example.Events/Program.cs) for a complete
working example.

## Related Resources

- [Amazon.Lambda.SQSEvents](https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.SQSEvents/README.md) –
  Base SQS event types
- [AwsLambda.Host](../AwsLambda.Host/README.md) – Core framework documentation

## License

This project is licensed under the MIT License. See [LICENSE](../../LICENSE) for details.
