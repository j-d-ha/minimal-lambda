# Envelopes

Envelope packages extend the official AWS Lambda event types (SQS, SNS, API Gateway, etc.) with
strongly typed payload accessors. Instead of deserializing JSON manually, you work with
`BodyContent<T>`, `MessageContent<T>`, or similar properties that the framework populates before your
handler executes.

Behind the scenes, aws-lambda-host injects the `UseExtractAndPackEnvelope` middleware at the end of
every pipeline. That middleware automatically calls `IRequestEnvelope.ExtractPayload` before your
handler runs and `IResponseEnvelope.PackPayload` after it finishes, guaranteeing consistent
serialization for both built-in envelopes and your own custom envelope types.

## Why Envelopes?

- **Strong typing** – `SqsEnvelope<Foo>` ensures handlers only run when payloads deserialize into
  `Foo`.
- **Zero boilerplate** – No more `JsonSerializer.Deserialize` calls sprinkled through handlers.
- **Consistent serialization** – `EnvelopeOptions` applies globally, including Native AOT
  `JsonSerializerContext` support.
- **Extensible** – Implement `IRequestEnvelope`/`IResponseEnvelope` for proprietary event shapes or
  alternative serialization formats (XML, Protobuf, etc.).

## Provided Envelopes

Install only the envelopes you need; each one lives in its own NuGet package.

| Event Source                    | Package                                                                                                                                                | NuGet                                                                                                                                                            |
|---------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| SQS                             | [AwsLambda.Host.Envelopes.Sqs](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/Envelopes/AwsLambda.Host.Envelopes.Sqs)                         | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs)                         |
| SNS                             | [AwsLambda.Host.Envelopes.Sns](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/Envelopes/AwsLambda.Host.Envelopes.Sns)                         | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns)                         |
| API Gateway / HTTP API          | [AwsLambda.Host.Envelopes.ApiGateway](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/Envelopes/AwsLambda.Host.Envelopes.ApiGateway)           | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway)           |
| Kinesis Data Streams            | [AwsLambda.Host.Envelopes.Kinesis](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/Envelopes/AwsLambda.Host.Envelopes.Kinesis)                 | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kinesis)                 |
| Kinesis Data Firehose           | [AwsLambda.Host.Envelopes.KinesisFirehose](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/Envelopes/AwsLambda.Host.Envelopes.KinesisFirehose) | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.KinesisFirehose) |
| Kafka (MSK / self-managed)      | [AwsLambda.Host.Envelopes.Kafka](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/Envelopes/AwsLambda.Host.Envelopes.Kafka)                     | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Kafka.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kafka)                     |
| CloudWatch Logs                 | [AwsLambda.Host.Envelopes.CloudWatchLogs](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/Envelopes/AwsLambda.Host.Envelopes.CloudWatchLogs)   | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.CloudWatchLogs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.CloudWatchLogs)   |
| Application Load Balancer (ALB) | [AwsLambda.Host.Envelopes.Alb](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/Envelopes/AwsLambda.Host.Envelopes.Alb)                         | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb)                         |

Each package ships with README examples in the repository if you need event-specific guidance.

## Quick Start

Install the envelope package that matches your event source, then use the envelope type in your
handler. This SQS example demonstrates the pattern; swap `SqsEnvelope<T>` with another envelope type
to handle SNS, API Gateway, etc.

```bash
dotnet add package AwsLambda.Host.Envelopes.Sqs
```

```csharp title="Program.cs" linenums="1"
using Amazon.Lambda.SQSEvents;
using AwsLambda.Host.Builder;
using AwsLambda.Host.Envelopes.Sqs;

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

lambda.MapHandler(
    ([Event] SqsEnvelope<OrderMessage> envelope, ILogger<Program> logger) =>
    {
        foreach (var record in envelope.Records)
        {
            if (record.BodyContent is null)
                continue;

            logger.LogInformation("Processing order {OrderId}", record.BodyContent.OrderId);
        }

        return new SQSBatchResponse(); // optional when you want to signal per-message failures
    }
);

await lambda.RunAsync();

internal sealed record OrderMessage(string OrderId, decimal Amount);
```

## Custom Serialization & EnvelopeOptions

All envelope packages respect the global `EnvelopeOptions` configuration. Call
`builder.Services.ConfigureEnvelopeOptions` to tweak `System.Text.Json` behavior, register
`JsonSerializerContext` instances for Native AOT, or enable XML readers/writers for custom envelopes.

```csharp title="Program.cs" linenums="1"
using System.Text.Json;
using System.Text.Json.Serialization;

builder.Services.ConfigureEnvelopeOptions(options =>
{
    options.JsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonOptions.TypeInfoResolver = MyEnvelopeJsonContext.Default; // Native AOT
});
```

The framework automatically copies `JsonOptions.TypeInfoResolver` into the internal
`LambdaDefaultJsonOptions`, so every envelope (including complex ones like CloudWatch Logs) gains the
same serialization metadata.

Need XML or another format? Set `options.XmlReaderSettings` / `XmlWriterSettings` and implement your
envelope using `System.Xml`. See the SQS README in the repo for a complete XML sample.

### Advanced Configuration

- **`LambdaDefaultJsonOptions`** – aws-lambda-host maintains a second `JsonSerializerOptions`
  instance for Lambda-specific envelopes (e.g., SNS→SQS fan-out). Most apps shouldn’t touch it; the
  host copies your `JsonOptions.TypeInfoResolver` automatically. Only override it when you need
  different converters for those hybrid envelopes.
- **`Items` dictionary** – Store arbitrary context for custom envelopes:

    ```csharp title="Program.cs" linenums="1"
    builder.Services.ConfigureEnvelopeOptions(options =>
    {
        options.Items["SchemaVersion"] = "v2";
        options.Items["Validator"] = new PayloadValidator();
    });
    ```

    Inside your envelope implementation, read `options.Items` to control validation or routing logic.

## Creating Custom Envelopes

Implement `IRequestEnvelope` and/or `IResponseEnvelope` when you control the event schema or need a
non-standard payload format. The middleware automatically invokes these interfaces, so you only write
the extraction logic.

```csharp title="CustomRequestEnvelope.cs" linenums="1"
using System.Text.Json;
using System.Text.Json.Serialization;
using AwsLambda.Host.Envelopes;
using AwsLambda.Host.Options;

public sealed class CustomRequestEnvelope : IRequestEnvelope
{
    [JsonPropertyName("payload")]
    public required string Payload { get; set; }

    [JsonIgnore]
    public MyPayload? PayloadContent { get; private set; }

    public void ExtractPayload(EnvelopeOptions options)
    {
        PayloadContent = JsonSerializer.Deserialize<MyPayload>(Payload, options.JsonOptions);
    }
}
```

In a handler:

```csharp title="Program.cs" linenums="1"
lambda.MapHandler(([Event] CustomRequestEnvelope envelope) =>
{
    if (envelope.PayloadContent is null)
        return new { Error = "Invalid payload" };

    return new { Success = true, envelope.PayloadContent.Name };
});
```

Response envelopes work the same way—implement `IResponseEnvelope` and serialize into a string
property inside `PackPayload`.

### Batch Envelopes

If your custom event contains multiple records (similar to SQS), deserialize each entry inside
`ExtractPayload`. Keep the original serialized string plus a `[JsonIgnore]` property for the strongly
typed object.

## Best Practices

- **Check for null** – Always guard against `BodyContent`/`PayloadContent` being `null`. Set it to
  `null` if deserialization fails instead of throwing.
- **Use `[JsonIgnore]`** – Keep serialized strings (`Body`, `Payload`, etc.) separate from the
  deserialized object to avoid recursive serialization.
- **Return `SQSBatchResponse` when required** – For SQS/SNS to SQS fan-out scenarios, populate
  `BatchItemFailures` to signal per-message errors.
- **Centralize configuration** – Prefer `ConfigureEnvelopeOptions` or configuration binding over
  ad-hoc serializer tweaks.
- **Log deserialization issues** – Logging helps diagnose malformed payloads without crashing the
  Lambda.

## When to Use Envelopes

Choose envelopes whenever:

- You want compile-time type checking for Lambda payloads.
- Your event contains nested JSON/XML that you’d otherwise deserialize manually.
- You need consistent serialization between request and response envelopes (API Gateway, ALB).
- You’re preparing for Native AOT and want the serialization metadata defined in one place.

Not every Lambda needs an envelope—handlers that already consume strongly typed SDK models can rely
on the base event types. Mix and match envelopes based on the triggers your project uses.
