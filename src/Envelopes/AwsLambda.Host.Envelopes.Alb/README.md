# AwsLambda.Host.Envelopes.Alb

Strongly-typed Application Load Balancer event handling for the AwsLambda.Host framework.

## Overview

This package provides strongly-typed envelopes for handling Application Load Balancer (ALB) events
in Lambda functions.
It contains classes that can be used as input and output types for Lambda functions that process
requests from an AWS Application Load Balancer.

The envelopes extend the base [
`ApplicationLoadBalancerRequest`](https://github.com/aws/aws-lambda-dotnet/tree/master/Libraries/src/Amazon.Lambda.ApplicationLoadBalancerEvents)
and [
`ApplicationLoadBalancerResponse`](https://github.com/aws/aws-lambda-dotnet/tree/master/Libraries/src/Amazon.Lambda.ApplicationLoadBalancerEvents)
with strongly-typed `BodyContent` properties for easier request/response
serialization:

| Envelope Class           | Base Class                        | Use Case                                    |
|--------------------------|-----------------------------------|---------------------------------------------|
| `AlbRequestEnvelope<T>`  | `ApplicationLoadBalancerRequest`  | ALB requests with deserialized body content |
| `AlbResponseEnvelope<T>` | `ApplicationLoadBalancerResponse` | ALB responses with typed body content       |

## Quick Start

Define your request and response types, then create a handler:

```csharp
using System;
using System.Collections.Generic;
using AwsLambda.Host.Builder;
using AwsLambda.Host.Envelopes.Alb;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

// AlbRequestEnvelope<Request> provides the ALB event with deserialized request body
// AlbResponseEnvelope<Response> wraps the response and serializes it to the body
lambda.MapHandler(
    ([Event] AlbRequestEnvelope<Request> request, ILogger<Program> logger) =>
    {
        logger.LogInformation("Request: {Name}", request.BodyContent?.Name);

        return new AlbResponseEnvelope<Response>
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

## Custom Envelopes

To implement custom deserialization logic, extend the appropriate base class and override the
payload handling method:

```csharp
// Example: Custom XML deserialization for requests
public sealed class XmlAlbRequestEnvelope<T> : AlbRequestEnvelopeBase<T>
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
public sealed class XmlAlbResponseEnvelope<T> : AlbResponseEnvelopeBase<T>
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
[JsonSerializable(typeof(AlbRequestEnvelope<Request>))]
[JsonSerializable(typeof(AlbResponseEnvelope<Response>))]
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
> deserialized at different steps: the Lambda serializer deserializes the ALB event, and the
> envelope options deserialize the request body and serialize the response body.

See the [example project](../../examples/AwsLambda.Host.Example.Events/Program.cs) for a complete
working example.

## Related Packages

| Package                                                                                               | NuGet                                                                                                                                                            | Downloads                                                                                                                                                              |
|-------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [**AwsLambda.Host.Envelopes.ApiGateway**](../AwsLambda.Host.Envelopes.ApiGateway/README.md)           | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway)           | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway/)           |
| [**AwsLambda.Host.Envelopes.Sqs**](../AwsLambda.Host.Envelopes.Sqs/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs/)                         |
| [**AwsLambda.Host.Envelopes.Sns**](../AwsLambda.Host.Envelopes.Sns/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns/)                         |
| [**AwsLambda.Host.Envelopes.Kinesis**](../AwsLambda.Host.Envelopes.Kinesis/README.md)                 | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kinesis)                 | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kinesis/)                 |
| [**AwsLambda.Host.Envelopes.KinesisFirehose**](../AwsLambda.Host.Envelopes.KinesisFirehose/README.md) | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.KinesisFirehose) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.KinesisFirehose/) |

## License

This project is licensed under the MIT License. See [LICENSE](../../LICENSE) for details.
