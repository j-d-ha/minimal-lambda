# Envelope

**What are Envelopes?**

Envelope packages wrap official AWS Lambda event classes (like `SQSEvent`, `APIGatewayProxyRequest`) and add a `BodyContent<T>` property that provides type-safe access to deserialized message payloads. Instead of manually parsing JSON strings from event bodies, you get strongly-typed objects with full IDE support and compile-time type checking.

**Key Benefits**:

- **Type Safety** - Generic type parameter `<T>` ensures compile-time type checking
- **Extensibility** - Abstract base classes allow custom serialization formats (JSON, XML, etc.)
- **Zero Overhead** - Envelopes extend official AWS event types, adding no runtime cost
- **AOT Ready** - Support for Native AOT compilation via `JsonSerializerContext` registration
- **Familiar API** - Works seamlessly with existing AWS Lambda event patterns

**Supported Event Sources**:

| Event Source                    | Package                                                                                                                                                | NuGet                                                                                                                                                            | Use Case                                                 |
|---------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------|
| **SQS**                         | [AwsLambda.Host.Envelopes.Sqs](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/Envelopes/AwsLambda.Host.Envelopes.Sqs)                        | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs)                         | Queue message processing with type-safe payloads         |
| **SNS**                         | [AwsLambda.Host.Envelopes.Sns](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/Envelopes/AwsLambda.Host.Envelopes.Sns)                        | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns)                         | Pub/sub notifications with typed messages                |
| **API Gateway**                 | [AwsLambda.Host.Envelopes.ApiGateway](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/Envelopes/AwsLambda.Host.Envelopes.ApiGateway)          | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway)           | REST/HTTP/WebSocket APIs with request/response envelopes |
| **Kinesis Data Streams**        | [AwsLambda.Host.Envelopes.Kinesis](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/Envelopes/AwsLambda.Host.Envelopes.Kinesis)                | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kinesis)                 | Stream processing with typed records                     |
| **Kinesis Data Firehose**       | [AwsLambda.Host.Envelopes.KinesisFirehose](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/Envelopes/AwsLambda.Host.Envelopes.KinesisFirehose) | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.KinesisFirehose) | Data transformation with typed payloads                  |
| **Kafka (MSK or self-managed)** | [AwsLambda.Host.Envelopes.Kafka](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/Envelopes/AwsLambda.Host.Envelopes.Kafka)                    | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Kafka.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kafka)                     | Event streaming with typed messages                      |
| **CloudWatch Logs**             | [AwsLambda.Host.Envelopes.CloudWatchLogs](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/Envelopes/AwsLambda.Host.Envelopes.CloudWatchLogs)  | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.CloudWatchLogs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.CloudWatchLogs)   | Log processing with typed log events                     |
| **Application Load Balancer**   | [AwsLambda.Host.Envelopes.Alb](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/Envelopes/AwsLambda.Host.Envelopes.Alb)                        | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb)                         | ALB target integration with request/response envelopes   |

!!! tip "Learn More About Envelopes"
    For detailed implementation examples and API documentation, see the individual package README files on GitHub (linked in the table above).

## Quick Start

### Using Envelopes

This example demonstrates the envelope pattern using SQS, but the same pattern applies to all envelope types (SNS, API Gateway, Kinesis, etc.) - simply swap `SqsEnvelope<T>` for the appropriate envelope type.

```csharp title="Program.cs" linenums="1"
using AwsLambda.Host.Builder;
using AwsLambda.Host.Envelopes.Sqs;

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

// Type-safe SQS message processing
lambda.MapHandler(
    ([Event] SqsEnvelope<OrderMessage> envelope, ILogger<Program> logger) =>
    {
        foreach (var record in envelope.Records)
        {
            if (record.BodyContent is null)
                continue;

            logger.LogInformation(
                "Processing order: {OrderId}",
                record.BodyContent.OrderId
            );
        }

        return new SQSBatchResponse();
    }
);

await lambda.RunAsync();

internal record OrderMessage(string OrderId, decimal Amount);
```

---

## Custom Serialization

Envelopes support custom serialization formats beyond JSON. By extending envelope base classes, you can implement XML, Protocol Buffers, or any other serialization format.

**Example use cases:**

- Legacy systems using XML message formats
- High-performance scenarios requiring Protocol Buffers
- Custom binary formats for specialized domains
- Integration with third-party systems using non-JSON formats

For implementation examples, see the [SQS Envelope README](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/Envelopes/AwsLambda.Host.Envelopes.Sqs#custom-envelopes) which demonstrates XML serialization.

---

## Configuration

Envelope configuration is managed through the `EnvelopeOptions` class, which controls how envelope payloads are serialized and deserialized. Configuration applies globally to all envelope types in your application.

### Quick Start

The most common configuration scenario is customizing JSON serialization:

```csharp title="Program.cs" linenums="1"
using System.Text.Json;

var builder = LambdaApplication.CreateBuilder();

builder.Services.ConfigureEnvelopeOptions(options =>
{
    // Use snake_case for JSON property names
    options.JsonOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;

    // Case-insensitive property matching
    options.JsonOptions.PropertyNameCaseInsensitive = true;

    // Allow trailing commas in JSON
    options.JsonOptions.AllowTrailingCommas = true;
});

var lambda = builder.Build();
```

---

### Configuration Properties

`EnvelopeOptions` provides five configuration properties for different serialization scenarios:

| Property | Type | Purpose | Default | When to Use |
|----------|------|---------|---------|-------------|
| **JsonOptions** | `JsonSerializerOptions` | JSON serialization/deserialization for envelope payloads | Empty options | Most common - configure naming policies, converters, AOT support |
| **LambdaDefaultJsonOptions** | `JsonSerializerOptions` | AWS Lambda-specific JSON settings for complex envelope payloads | AWS defaults¹ | Automatic - rarely needs manual configuration |
| **XmlReaderSettings** | `XmlReaderSettings` | XML deserialization settings | Default settings | Custom envelopes using XML serialization |
| **XmlWriterSettings** | `XmlWriterSettings` | XML serialization settings | Default settings | Custom envelopes using XML serialization |
| **Items** | `Dictionary<object, object>` | Custom extension data | Empty dictionary | Advanced - store custom context for envelope processing |

¹ *AWS defaults include: case-insensitive property matching, AWS naming policy (PascalCase), DateTime/MemoryStream/ByteArray converters*

!!! info "LambdaDefaultJsonOptions Behavior"
    `LambdaDefaultJsonOptions` is automatically configured with AWS Lambda-compatible settings and is used internally for complex envelope types like SNS-to-SQS and CloudWatch Logs. During post-configuration, the framework automatically copies `JsonOptions.TypeInfoResolver` to `LambdaDefaultJsonOptions` to ensure AOT compatibility works correctly across all envelope types.

---

### JSON Configuration

Configure JSON serialization for envelope payloads:

```csharp title="Common JSON patterns" linenums="1"
using System.Text.Json;
using System.Text.Json.Serialization;

builder.Services.ConfigureEnvelopeOptions(options =>
{
    // Naming policies
    options.JsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

    // Write indented JSON for debugging
    options.JsonOptions.WriteIndented = true;

    // Handle null values
    options.JsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

    // Number handling
    options.JsonOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;

    // Custom converters
    options.JsonOptions.Converters.Add(new MyCustomConverter());
});
```

#### Native AOT Support

For Native AOT compilation, register a `JsonSerializerContext`:

```csharp title="AOT-compatible configuration" linenums="1"
using System.Text.Json.Serialization;

// Define your JSON source generation context
[JsonSerializable(typeof(OrderMessage))]
[JsonSerializable(typeof(SqsEnvelope<OrderMessage>))]
internal partial class MyJsonContext : JsonSerializerContext { }

var builder = LambdaApplication.CreateBuilder();

builder.Services.ConfigureEnvelopeOptions(options =>
{
    // Register source generation context for AOT
    options.JsonOptions.TypeInfoResolver = MyJsonContext.Default;
});

var lambda = builder.Build();
```

!!! tip "AOT Compatibility"
    The framework automatically copies `TypeInfoResolver` from `JsonOptions` to `LambdaDefaultJsonOptions`, ensuring all envelope types work correctly with Native AOT compilation.

---

### XML Configuration

For custom envelopes using XML serialization (see [Custom Serialization](#custom-serialization)):

```csharp title="XML configuration" linenums="1"
using System.Xml;

builder.Services.ConfigureEnvelopeOptions(options =>
{
    // XML reader settings (deserialization)
    options.XmlReaderSettings.DtdProcessing = DtdProcessing.Prohibit;
    options.XmlReaderSettings.IgnoreWhitespace = true;
    options.XmlReaderSettings.IgnoreComments = true;

    // XML writer settings (serialization)
    options.XmlWriterSettings.Indent = true;
    options.XmlWriterSettings.IndentChars = "  ";
    options.XmlWriterSettings.OmitXmlDeclaration = false;
});
```

---

### Advanced: Custom Extension Data

The `Items` dictionary allows storing custom context or configuration data for envelope processing:

```csharp title="Using Items dictionary" linenums="1"
builder.Services.ConfigureEnvelopeOptions(options =>
{
    // Store custom context for envelope implementations
    options.Items["SchemaVersion"] = "2.0";
    options.Items["ValidationEnabled"] = true;
    options.Items["CustomProcessor"] = new MyCustomProcessor();
});
```

**Example: Accessing Items in a custom envelope**

```csharp title="CustomEnvelope.cs" linenums="1"
public class CustomEnvelope : IRequestEnvelope
{
    public void ExtractPayload(EnvelopeOptions options)
    {
        // Access custom configuration
        if (options.Items.TryGetValue("ValidationEnabled", out var enabled)
            && enabled is true)
        {
            ValidatePayload();
        }

        // Deserialize payload...
    }
}
```

---

## Creating Custom Envelopes

Custom envelopes allow you to define your own event types with nested payloads that are automatically extracted and packed by the framework. This is useful when you need custom event structures beyond AWS's standard event types.

### When to Use Custom Envelopes

Create custom envelopes when:

- ✅ You're defining your own event structure (not using AWS events like SQS, SNS, etc.)
- ✅ Your event contains a serialized payload that needs deserialization
- ✅ You want automatic payload extraction/packing by the framework
- ✅ You need type-safe access to nested event data

!!! note "Custom Serialization vs Custom Envelopes"
    **Custom envelopes** = Your own event types implementing `IRequestEnvelope`/`IResponseEnvelope`

    **Custom serialization** = Using XML, Protobuf, etc. with existing AWS events (see the [SQS Envelope README](https://github.com/j-d-ha/aws-lambda-host/tree/main/src/Envelopes/AwsLambda.Host.Envelopes.Sqs#custom-envelopes) for examples)

### Core Interfaces

#### IRequestEnvelope

Implement this interface for incoming events that contain nested payloads:

```csharp
public interface IRequestEnvelope
{
    void ExtractPayload(EnvelopeOptions options);
}
```

The framework automatically calls `ExtractPayload()` **before** your handler executes, allowing you to deserialize nested data.

#### IResponseEnvelope

Implement this interface for outgoing responses that need serialization:

```csharp
public interface IResponseEnvelope
{
    void PackPayload(EnvelopeOptions options);
}
```

The framework automatically calls `PackPayload()` **after** your handler executes, serializing your response data.

---

### Example 1: Simple Request Envelope

A custom event with metadata and a nested JSON payload:

```csharp title="CustomRequestEvent.cs" linenums="1"
using System.Text.Json;
using System.Text.Json.Serialization;
using AwsLambda.Host.Abstractions.Options;
using AwsLambda.Host.Envelopes;

public class CustomRequestEvent : IRequestEnvelope
{
    [JsonPropertyName("metadata")]
    public required EventMetadata Metadata { get; set; }

    [JsonPropertyName("payload")]
    public required string Payload { get; set; }  // Serialized JSON string

    [JsonIgnore]
    public MyData? PayloadContent { get; set; }  // Deserialized object

    public void ExtractPayload(EnvelopeOptions options)
    {
        PayloadContent = JsonSerializer.Deserialize<MyData>(Payload, options.JsonOptions);
    }
}

public record EventMetadata(string EventId, DateTime Timestamp);
public record MyData(string Name, int Value);
```

**Usage:**

```csharp title="Program.cs" linenums="1"
var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

lambda.MapHandler(([Event] CustomRequestEvent request, ILogger<Program> logger) =>
{
    if (request.PayloadContent is null)
    {
        logger.LogError("Failed to deserialize payload");
        return new { Error = "Invalid payload" };
    }

    logger.LogInformation(
        "Processing event {EventId}: {Name} = {Value}",
        request.Metadata.EventId,
        request.PayloadContent.Name,
        request.PayloadContent.Value
    );

    return new { Success = true };
});

await lambda.RunAsync();
```

---

### Example 2: Request/Response Envelope Pair

Custom envelopes for both incoming requests and outgoing responses:

```csharp title="ApiEnvelopes.cs" linenums="1"
using System.Text.Json;
using System.Text.Json.Serialization;
using AwsLambda.Host.Abstractions.Options;
using AwsLambda.Host.Envelopes;

// Request envelope
public class ApiRequest : IRequestEnvelope
{
    [JsonPropertyName("body")]
    public required string Body { get; set; }

    [JsonIgnore]
    public RequestPayload? BodyContent { get; set; }

    public void ExtractPayload(EnvelopeOptions options)
    {
        BodyContent = JsonSerializer.Deserialize<RequestPayload>(Body, options.JsonOptions);
    }
}

// Response envelope
public class ApiResponse : IResponseEnvelope
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonIgnore]
    public ResponsePayload? BodyContent { get; set; }

    public void PackPayload(EnvelopeOptions options)
    {
        Body = JsonSerializer.Serialize(BodyContent, options.JsonOptions);
    }
}

public record RequestPayload(string Action, Dictionary<string, string> Parameters);
public record ResponsePayload(bool Success, string Message, object? Data = null);
```

**Usage:**

```csharp title="Program.cs" linenums="1"
lambda.MapHandler<ApiRequest, ApiResponse>(request =>
{
    var response = new ApiResponse { StatusCode = 200 };

    if (request.BodyContent is null)
    {
        response.StatusCode = 400;
        response.BodyContent = new ResponsePayload(
            Success: false,
            Message: "Invalid request body"
        );
        return response;
    }

    // Process the request
    var result = ProcessAction(request.BodyContent.Action, request.BodyContent.Parameters);

    response.BodyContent = new ResponsePayload(
        Success: true,
        Message: "Action completed",
        Data: result
    );

    return response;
});
```

---

### Example 3: Multi-Record Batch Envelope

A custom event with multiple records, similar to SQS batch processing:

```csharp title="BatchEvent.cs" linenums="1"
using System.Text.Json;
using System.Text.Json.Serialization;
using AwsLambda.Host.Abstractions.Options;
using AwsLambda.Host.Envelopes;

public class BatchEvent : IRequestEnvelope
{
    [JsonPropertyName("records")]
    public required List<Record> Records { get; set; }

    public void ExtractPayload(EnvelopeOptions options)
    {
        foreach (var record in Records)
        {
            try
            {
                record.DataContent = JsonSerializer.Deserialize<MyPayload>(
                    record.Data,
                    options.JsonOptions
                );
            }
            catch
            {
                record.DataContent = null;  // Handle deserialization failures gracefully
            }
        }
    }

    public class Record
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("data")]
        public required string Data { get; set; }  // Serialized JSON

        [JsonIgnore]
        public MyPayload? DataContent { get; set; }  // Deserialized object
    }
}

public record MyPayload(string Type, int Count, DateTime CreatedAt);
```

**Usage:**

```csharp title="Program.cs" linenums="1"
lambda.MapHandler(([Event] BatchEvent batch, ILogger<Program> logger) =>
{
    var processed = 0;
    var failed = 0;

    foreach (var record in batch.Records)
    {
        if (record.DataContent is null)
        {
            logger.LogWarning("Failed to deserialize record {RecordId}", record.Id);
            failed++;
            continue;
        }

        logger.LogInformation(
            "Processing record {RecordId}: {Type} x{Count}",
            record.Id,
            record.DataContent.Type,
            record.DataContent.Count
        );

        ProcessRecord(record.DataContent);
        processed++;
    }

    return new { Processed = processed, Failed = failed };
});
```

---

### Best Practices

#### ✅ Do

- **Check for null after extraction** - Deserialization can fail
- **Use `[JsonIgnore]` on content properties** - Prevents circular serialization
- **Handle exceptions gracefully** - Set content to `null` on deserialization errors
- **Use `EnvelopeOptions`** - Access configured JSON/XML settings through the `options` parameter
- **Log deserialization failures** - Helps with debugging malformed payloads

#### ❌ Don't

- **Don't assume `PayloadContent` is non-null** - Always check before using
- **Don't throw exceptions in `ExtractPayload`** - Handle errors gracefully
- **Don't modify the original payload string** - Only populate the deserialized content property

---

### Key Concepts

1. **Automatic Invocation** - The framework calls `ExtractPayload()` and `PackPayload()` automatically via middleware

2. **Separation of Concerns** - Keep serialized strings and deserialized objects separate using `[JsonIgnore]`

3. **Configuration Access** - Use `options.JsonOptions`, `options.XmlReaderSettings`, etc. for consistent serialization settings

4. **Error Handling** - Set content properties to `null` when deserialization fails rather than throwing exceptions

---

## Choosing the Right Feature

### When to Use Envelopes

Use envelope packages when:

- ✅ You need type-safe access to message payloads from AWS event sources
- ✅ You want compile-time type checking for event data
- ✅ You're tired of manually parsing JSON from event bodies
- ✅ You need custom serialization formats (XML, Protobuf, etc.)
- ✅ You want IDE IntelliSense support for message structures

---

## Installation

### Envelope Packages 

Install only the envelope packages you need:

```bash
# SQS envelope
dotnet add package AwsLambda.Host.Envelopes.Sqs

# API Gateway envelope
dotnet add package AwsLambda.Host.Envelopes.ApiGateway

# Other envelopes...
```
