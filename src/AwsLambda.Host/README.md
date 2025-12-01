# AwsLambda.Host

Core framework for building AWS Lambda functions with dependency injection, middleware, and source
generation.

> 📚 **[View Full Documentation](https://j-d-ha.github.io/aws-lambda-host/)**

## Overview

A modern .NET framework for building AWS Lambda functions using familiar ASP.NET Core patterns. The
core runtime provides:

- **Dependency Injection**: Built-in service container for managing application dependencies
- **Middleware Pipeline**: Request/response processing similar to ASP.NET Core middleware
- **Compile-time Code Generation**: Source generators reduce reflection overhead and improve startup
  performance
- **Native AOT Support**: Full compatibility with ahead-of-time compilation for minimal cold starts
  and reduced package size
- **Lambda-Optimized Design**: Event handling, cold start reduction, and efficient resource
  utilization tailored to AWS Lambda constraints

## Installation

Install the NuGet package:

```bash
dotnet add package AwsLambda.Host
```

Ensure your project uses C# 11 or later:

```xml

<PropertyGroup>
  <LangVersion>11</LangVersion>
  <!-- or <LangVersion>latest</LangVersion> -->
</PropertyGroup>
```

## Quick Start

Create a simple Lambda handler:

```csharp
using AwsLambda.Host.Builder;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

// The [Event] attribute marks the parameter that receives the deserialized Lambda event
lambda.MapHandler(([Event] string input) => $"Hello {input}!");

await lambda.RunAsync();
```

The `[Event]` attribute tells the framework which parameter receives the deserialized event. You can
also inject dependencies:

```csharp
lambda.MapHandler(([Event] Order order, IOrderService service) =>
    service.Process(order)
);
```

Add middleware for cross-cutting concerns:

```csharp
lambda.UseMiddleware(async (context, next) =>
{
    Console.WriteLine("Before handler");
    await next(context);
    Console.WriteLine("After handler");
});
```

Use `OnInit()` for setup and `OnShutdown()` for cleanup:

```csharp
// Service can be injected into the Init handler
lambda.OnInit(ICache cache =>
{
    // Runs once at startup - perfect for setting up resources
    cache.Warm();
});

// Handlers can also control if the Init phase should be continued or not
lambda.OnInit(async (services, token) =>
{
    // Returns false to abort startup
    return true;
});

// Runs once at shutdown - cleanup resources
lambda.OnShutdown(async (services, token) =>
{
    // ...
});

// Service can be injected into the shutdown handler too handler
lambda.OnShutdown(ITelemetryService telemetryService =>
{
    // Runs once at shutdown - great for cleaning up resources
    telemetryService.ForceFlush();
});
```

## Key Features

- **Source Generators** – Compile-time code generation eliminates reflection; zero runtime overhead
- **Interceptors** – Handler parameters resolved at compile time, not runtime
- **Dependency Injection** – Built-in scoped lifetime management per invocation
- **Middleware Pipeline** – Familiar ASP.NET Core-style middleware for cross-cutting concerns
- **AOT Ready** – Full support for .NET Native AOT compilation
- **Lambda Lifecycle** – Explicit control over Init, Invocation, and Shutdown phases
- **Automatic Cancellation** – Cancellation tokens respect Lambda timeout with configurable buffer

## Core Concepts

### Handlers & Middleware

Register your Lambda handler with the builder. The framework uses source generation to analyze your
handler signature:

- The `[Event]` attribute marks the input parameter type
- The return type determines the response type
- Source generation handles serialization/deserialization automatically

Handlers can inject dependencies alongside the event:

```csharp
lambda.MapHandler(([Event] Order order, IOrderService service) =>
    service.Process(order)  // Return type automatically serialized to JSON
);
```

Middleware wraps the handler for cross-cutting concerns. Add as many middlewares as needed—they
compose into a pipeline:

```csharp
lambda.UseMiddleware(async (context, next) =>
{
    // Pre-handler logic
    await next(context);
    // Post-handler logic
});

lambda.UseMiddleware(async (context, next) =>
{
    // Another middleware layer
    await next(context);
});
```

### Lambda Lifecycle

The framework manages initialization and shutdown phases automatically. Add as many callbacks as
needed—they execute in order and then all awaited:

- **OnInit** – Runs once when the function initializes; ideal for setting up resources like database
  connections
- **OnShutdown** – Runs once before Lambda terminates; cleanup and resource release

Both run asynchronously and should be kept as short as possible to minimize startup/shutdown time.

```csharp
lambda.OnInit(async (services, token) =>
{
    // One-time setup (runs once, reused across invocations)
    return true; // or false to abort startup
});

lambda.OnShutdown(async (services, token) =>
{
    // Cleanup before shutdown
});
```

### Dependency Injection

Register services in the builder; they're available in handlers, middleware, and lifecycle methods:

```csharp
builder.Services.AddSingleton<ICache, MemoryCache>();      // Reused across invocations
builder.Services.AddScoped<IRepository, Repository>();    // New per invocation
```

Each invocation receives its own scope—scoped services are isolated per request. `OnInit()` and
`OnShutdown()` handlers receive their own scopes as well. You can also request the
`ILambdaHostContext` or `CancellationToken` in any handler, and they're automatically injected.

### Source Generation & Interceptors

The framework uses C# source generators and compile-time interceptors to:

- Analyze handler signatures at compile time
- Generate optimized dependency injection code
- Resolve handler parameters without reflection

Result: **Zero runtime reflection, zero performance cost.**

### AOT Support

To use .NET Native AOT, define a JSON serializer context and annotate with types to serialize:

```csharp
using System.Text.Json.Serialization;

[JsonSerializable(typeof(string))]
public partial class SerializerContext : JsonSerializerContext;
```

Register the serializer context with the application:

```csharp
using AwsLambda.Host;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddLambdaSerializerWithContext<SerializerContext>();

var lambda = builder.Build();
```

The `AddLambdaSerializerWithContext<TContext>()` method registers a source-generated JSON serializer
that uses your context for all Lambda event and response serialization, providing compile-time
serialization metadata and eliminating runtime reflection.

Enable AOT in your project file:

```xml
<PublishAot>true</PublishAot>
<PublishTrimmed>true</PublishTrimmed>
<TrimMode>full</TrimMode>
<JsonSerializerIsReflectionEnabledByDefault>false</JsonSerializerIsReflectionEnabledByDefault>
```

See [AOT documentation](https://docs.microsoft.com/en-us/dotnet/fundamentals/aot/overview) for
details.

## Configuration

The framework supports configuration through `LambdaHostOptions`:

```csharp
builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.InitTimeout = TimeSpan.FromSeconds(10);
    options.InvocationCancellationBuffer = TimeSpan.FromSeconds(5);
    options.ShutdownDuration = ShutdownDuration.ExternalExtensions;
    options.ShutdownDurationBuffer = TimeSpan.FromMilliseconds(100);
    options.ClearLambdaOutputFormatting = true;
});
```

Available options include timeout control, shutdown duration, output formatting, and JSON
serialization customization. The framework automatically registers `DefaultLambdaHostJsonSerializer`
which uses `JsonSerializerOptions` and `JsonWriterOptions` for all Lambda serialization. See
the [configuration guide](https://github.com/j-d-ha/aws-lambda-host/wiki/Configuration) for details.

## Other Packages

Additional packages in the aws-lambda-host framework for abstractions, observability, and event
source handling.

| Package                                                                                                         | NuGet                                                                                                                                                            | Downloads                                                                                                                                                              |
|-----------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [**AwsLambda.Host**](../AwsLambda.Host/README.md)                                                               | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.svg)](https://www.nuget.org/packages/AwsLambda.Host)                                                     | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.svg)](https://www.nuget.org/packages/AwsLambda.Host/)                                                     |
| [**AwsLambda.Host.Abstractions**](../AwsLambda.Host.Abstractions/README.md)                                     | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Abstractions.svg)](https://www.nuget.org/packages/AwsLambda.Host.Abstractions)                           | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Abstractions.svg)](https://www.nuget.org/packages/AwsLambda.Host.Abstractions/)                           |
| [**AwsLambda.Host.OpenTelemetry**](../AwsLambda.Host.OpenTelemetry/README.md)                                   | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.OpenTelemetry.svg)](https://www.nuget.org/packages/AwsLambda.Host.OpenTelemetry)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.OpenTelemetry.svg)](https://www.nuget.org/packages/AwsLambda.Host.OpenTelemetry/)                         |
| [**AwsLambda.Host.Envelopes.Sqs**](../Envelopes/AwsLambda.Host.Envelopes.Sqs/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs/)                         |
| [**AwsLambda.Host.Envelopes.ApiGateway**](../Envelopes/AwsLambda.Host.Envelopes.ApiGateway/README.md)           | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway)           | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway/)           |
| [**AwsLambda.Host.Envelopes.Sns**](../Envelopes/AwsLambda.Host.Envelopes.Sns/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns/)                         |
| [**AwsLambda.Host.Envelopes.Kinesis**](../Envelopes/AwsLambda.Host.Envelopes.Kinesis/README.md)                 | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kinesis)                 | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kinesis/)                 |
| [**AwsLambda.Host.Envelopes.KinesisFirehose**](../Envelopes/AwsLambda.Host.Envelopes.KinesisFirehose/README.md) | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.KinesisFirehose) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.KinesisFirehose/) |
| [**AwsLambda.Host.Envelopes.Kafka**](../Envelopes/AwsLambda.Host.Envelopes.Kafka/README.md)                     | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Kafka.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kafka)                     | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Kafka.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kafka/)                     |
| [**AwsLambda.Host.Envelopes.CloudWatchLogs**](../Envelopes/AwsLambda.Host.Envelopes.CloudWatchLogs/README.md)   | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.CloudWatchLogs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.CloudWatchLogs)   | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.CloudWatchLogs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.CloudWatchLogs/)   |
| [**AwsLambda.Host.Envelopes.Alb**](../Envelopes/AwsLambda.Host.Envelopes.Alb/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb/)                         |

## License

This project is licensed under the MIT License. See [LICENSE](../../LICENSE) for details.
