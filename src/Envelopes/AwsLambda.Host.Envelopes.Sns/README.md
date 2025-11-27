# AwsLambda.Host.Envelopes.Sns

Strongly-typed SNS event handling for the AwsLambda.Host framework.

## Overview

This package provides `SnsEnvelope<T>`, which extends the base [
`SNSEvent`](https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.SNSEvents/README.md)
class with a generic `Records` collection that deserializes message bodies into strongly-typed
objects. Instead of manually parsing JSON from `record.Sns.Message`, you access deserialized
payloads directly via `record.BodyContent`.

| Envelope Class   | Base Class | Use Case                                   |
|------------------|------------|--------------------------------------------|
| `SnsEnvelope<T>` | `SNSEvent` | SNS event with deserialized message bodies |

## Quick Start

Define your message type and handler:

```csharp
using Amazon.Lambda.SNSEvents;
using AwsLambda.Host.Builder;
using AwsLambda.Host.Envelopes.Sns;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

// SnsEnvelope<Message> provides access to the SNS event and deserialized Message payloads
lambda.MapHandler(
    ([Event] SnsEnvelope<Message> envelope, ILogger<Program> logger) =>
    {
        foreach (var record in envelope.Records)
        {
            logger.LogInformation("Message: {Content}", record.BodyContent?.Content);
        }
    }
);

await lambda.RunAsync();

// Your message payload - will be deserialized from SNS message body
internal record Message(string Content);
```

## Custom Envelopes

To implement custom deserialization logic, extend `SnsEnvelopeBase<T>` and override the
`ExtractPayload` method:

```csharp
// Example: Custom XML deserialization
public sealed class SnsXmlEnvelope<T> : SnsEnvelopeBase<T>
{
    private static readonly XmlSerializer Serializer = new(typeof(T));

    public override void ExtractPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
        {
            using var stringReader = new StringReader(record.Sns.Message);
            using var xmlReader = XmlReader.Create(stringReader, options.XmlReaderSettings);
            record.BodyContent = (T)Serializer.Deserialize(xmlReader)!;
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
[JsonSerializable(typeof(SnsEnvelope<Message>))]
[JsonSerializable(typeof(Message))]
internal partial class SerializerContext : JsonSerializerContext;
```

Register the serializer and configure envelope options to use the context:

```csharp
builder.Services.AddLambdaSerializerWithContext<SerializerContext>();
```

See the [AwsLambda.Host documentation](https://github.com/j-d-ha/aws-lambda-host) for more details
on AOT support.

## Related Packages

- [AwsLambda.Host.Envelopes.Sqs](../AwsLambda.Host.Envelopes.Sqs/README.md) - SQS event handling
- [AwsLambda.Host.Envelopes.ApiGateway](../AwsLambda.Host.Envelopes.ApiGateway/README.md) - API
  Gateway event handling
- [AwsLambda.Host.Envelopes.Alb](../AwsLambda.Host.Envelopes.Alb/README.md) - Application Load
  Balancer event handling
