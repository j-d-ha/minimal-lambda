# MinimalLambda.Envelopes.ApiGateway

Strongly-typed API Gateway event handling for the MinimalLambda framework.

## Overview

This package provides strongly-typed envelopes for handling API Gateway events in Lambda functions.
It contains classes that can be used as input and output types for Lambda functions that process
REST APIs, HTTP APIs (payload format 1.0), WebSocket APIs, HTTP APIs (payload format 2.0), and
Lambda Function URLs.

The envelopes extend the base [
`APIGatewayProxyRequest`](https://github.com/aws/aws-lambda-dotnet/tree/master/Libraries/src/Amazon.Lambda.APIGatewayEvents), [
`APIGatewayProxyResponse`](https://github.com/aws/aws-lambda-dotnet/tree/master/Libraries/src/Amazon.Lambda.APIGatewayEvents),
and HTTP API equivalents with strongly-typed `BodyContent` properties for easier request/response
serialization:

| Envelope Class                    | Base Class                         | Use Case                                                                                |
|-----------------------------------|------------------------------------|-----------------------------------------------------------------------------------------|
| `ApiGatewayRequestEnvelope<T>`    | `APIGatewayProxyRequest`           | REST API, HTTP API payload format 1.0, or WebSocket API requests with deserialized body |
| `ApiGatewayResponseEnvelope<T>`   | `APIGatewayProxyResponse`          | REST API, HTTP API payload format 1.0, or WebSocket API responses with typed body       |
| `ApiGatewayV2RequestEnvelope<T>`  | `APIGatewayHttpApiV2ProxyRequest`  | HTTP API payload format 2.0 requests with deserialized body                             |
| `ApiGatewayV2ResponseEnvelope<T>` | `APIGatewayHttpApiV2ProxyResponse` | HTTP API payload format 2.0 responses with typed body                                   |

## Quick Start

Define your request and response types, then create a handler:

```csharp
using System;
using System.Collections.Generic;
using MinimalLambda.Builder;
using MinimalLambda.Envelopes.ApiGateway;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

// ApiGatewayRequestEnvelope<Request> provides the API Gateway event with deserialized request body
// ApiGatewayResponseEnvelope<Response> wraps the response and serializes it to the body
lambda.MapHandler(
    ([Event] ApiGatewayRequestEnvelope<Request> request, ILogger<Program> logger) =>
    {
        logger.LogInformation("Request: {Name}", request.BodyContent?.Name);

        return new ApiGatewayResponseEnvelope<Response>
        {
            BodyContent = new Response($"Hello {request.BodyContent?.Name}!", DateTime.UtcNow),
            StatusCode = 200,
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" },
        };
    }
);

await lambda.RunAsync();

// Your request and response payloads
internal record Request(string Name);

internal record Response(string Message, DateTime TimestampUtc);
```

For HTTP API v2, use `ApiGatewayV2RequestEnvelope<T>` and `ApiGatewayV2ResponseEnvelope<T>` in the
same way.

## Custom Envelopes

To implement custom deserialization logic, extend the appropriate base class and override the
payload handling method:

```csharp
// Example: Custom XML deserialization for requests
public sealed class XmlApiGatewayXmlRequestEnvelope<T> : ApiGatewayRequestEnvelopeBase<T>
{
    public override void ExtractPayload(EnvelopeOptions options)
    {
        using var stringReader = new StringReader(Body);
        using var xmlReader = XmlReader.Create(stringReader, options.XmlReaderSettings);
        var serializer = new XmlSerializer(typeof(T));
        BodyContent = (T)serializer.Deserialize(xmlReader)!;
    }
}

// Example: Custom XML serialization for responses
public sealed class XmlApiGatewayXmlResponseEnvelope<T> : ApiGatewayResponseEnvelopeBase<T>
{
    public override void PackPayload(EnvelopeOptions options)
    {
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, options.XmlWriterSettings);
        var serializer = new XmlSerializer(typeof(T));
        serializer.Serialize(xmlWriter, BodyContent);
        Body = stringWriter.ToString();
    }
}
```

This pattern allows you to support multiple serialization formats while maintaining the same
envelope interface.

## AOT Support

When using .NET Native AOT, register all envelope and payload types in your `JsonSerializerContext`:

```csharp
[JsonSerializable(typeof(ApiGatewayRequestEnvelope<Request>))]
[JsonSerializable(typeof(ApiGatewayResponseEnvelope<Response>))]
[JsonSerializable(typeof(Request))]
[JsonSerializable(typeof(Response))]
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

| Package                                                                                               | NuGet                                                                                                                                                            | Downloads                                                                                                                                                              |
|-------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [**MinimalLambda**](../../MinimalLambda/README.md)                                                  | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.svg)](https://www.nuget.org/packages/MinimalLambda)                                                     | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.svg)](https://www.nuget.org/packages/MinimalLambda/)                                                     |
| [**MinimalLambda.Abstractions**](../../MinimalLambda.Abstractions/README.md)                        | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Abstractions.svg)](https://www.nuget.org/packages/MinimalLambda.Abstractions)                           | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Abstractions.svg)](https://www.nuget.org/packages/MinimalLambda.Abstractions/)                           |
| [**MinimalLambda.OpenTelemetry**](../../MinimalLambda.OpenTelemetry/README.md)                      | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.OpenTelemetry.svg)](https://www.nuget.org/packages/MinimalLambda.OpenTelemetry)                         | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.OpenTelemetry.svg)](https://www.nuget.org/packages/MinimalLambda.OpenTelemetry/)                         |
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
