# AwsLambda.Host.Envelopes.Sqs

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
using Amazon.Lambda.SQSEvents;
using AwsLambda.Host.Builder;
using AwsLambda.Host.Envelopes.Sqs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

// SqsEnvelope<Message> provides access to the SQS event and deserialized Message payloads
lambda.MapHandler(
    ([Event] SqsEnvelope<Message> envelope, ILogger<Program> logger) =>
    {
        // Inorder to handle any errors or unprocessed messages, you must return a SQSBatchResponse
        var batchResponse = new SQSBatchResponse();

        foreach (var record in envelope.Records)
        {
            // For this example, we'll add a failure to the batch response if the message body is null
            if (record.BodyContent is null)
                batchResponse.BatchItemFailures.Add(
                    new SQSBatchResponse.BatchItemFailure { ItemIdentifier = record.MessageId }
                );

            logger.LogInformation("Message: {Name}", record.BodyContent?.Name);
        }

        // Return the batch response regardless of whether there were any failures
        return batchResponse;
    }
);

await lambda.RunAsync();

// Your message payload - will be deserialized from SQS message body
internal record Message(string Name);
```

## Custom Envelopes

To implement custom deserialization logic, extend `SqsEnvelopeBase<T>` and override the
`ExtractPayload` method:

```csharp
// Example: Custom XML deserialization
public sealed class SqsXmlEnvelope<T> : SqsEnvelopeBase<T>
{
    private static readonly XmlSerializer Serializer = new(typeof(T));

    public override void ExtractPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
        {
            using var stringReader = new StringReader(record.Body);
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

## Related Packages

| Package                                                                                     | NuGet                                                                                                                                                  | Downloads                                                                                                                                                    |
|---------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [**AwsLambda.Host.Envelopes.ApiGateway**](../AwsLambda.Host.Envelopes.ApiGateway/README.md) | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway/) |
| [**AwsLambda.Host.Envelopes.Alb**](../AwsLambda.Host.Envelopes.Alb/README.md)               | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb)               | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb/)               |

## License

This project is licensed under the MIT License. See [LICENSE](../../LICENSE) for details.
