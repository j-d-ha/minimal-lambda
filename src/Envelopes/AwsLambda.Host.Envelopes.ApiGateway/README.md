# AwsLambda.Host.Envelopes.ApiGateway

Strongly-typed API Gateway event handling for the AwsLambda.Host framework.

## Overview

This package provides strongly-typed envelopes for handling API Gateway events in Lambda functions.
It contains classes that can be used as input and output types for Lambda functions that process
REST APIs, HTTP APIs (payload format 1.0), WebSocket APIs, and HTTP APIs (payload format 2.0).

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
using AwsLambda.Host.Builder;
using AwsLambda.Host.Envelopes.ApiGateway;
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

> **Note:** The context must be registered in both places because the Lambda event and payload are
> deserialized at different steps: the Lambda serializer deserializes the API Gateway event, and the
> envelope options deserialize the request body and serialize the response body.

See the [example project](../../examples/AwsLambda.Host.Example.Events/Program.cs) for a complete
working example.

## Related Packages

| Package                                                                       | NuGet                                                                                                                                    | Downloads                                                                                                                                      |
|-------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------|
| [**AwsLambda.Host.Envelopes.Alb**](../AwsLambda.Host.Envelopes.Alb/README.md) | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb/) |
| [**AwsLambda.Host.Envelopes.Sqs**](../AwsLambda.Host.Envelopes.Sqs/README.md) | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs/) |

## License

This project is licensed under the MIT License. See [LICENSE](../../LICENSE) for details.
