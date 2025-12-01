# OpenTelemetry

## What is OpenTelemetry Integration?

The `AwsLambda.Host.OpenTelemetry` package provides seamless integration with the official [`OpenTelemetry.Instrumentation.AWSLambda`](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AWSLambda) package. It acts as a smart adapter layer for `AwsLambda.Host`, using **C# 12 interceptors and source generation** to automatically instrument your Lambda handlers with minimal overhead.

At compile time, it wraps your handler invocation in a root trace span, providing reflection-free, high-performance distributed tracing.

---

## Key Benefits

- **Zero-Overhead Instrumentation**: Uses source generation to create trace spans for your handler at compile time, avoiding runtime reflection.
- **Distributed Tracing**: Propagates trace context across services, giving you end-to-end visibility in a microservices architecture.
- **Lifecycle Integration**: Provides an explicit shutdown hook to ensure all buffered telemetry is flushed before the Lambda execution environment is frozen.
- **Vendor-Neutral**: Fully compatible with any OpenTelemetry-compliant backend, including Jaeger, Datadog, New Relic, Honeycomb, and AWS X-Ray.
- **Standard APIs**: Leverages the standard OpenTelemetry .NET SDK, so you can use familiar configuration and custom instrumentation APIs.
- **Custom Instrumentation**: Easily create custom spans (`Activity`) and metrics (`Meter`) to capture application-specific logic.

---

## Quick Start

### Installation

Install the OpenTelemetry integration package and any required exporter packages.

```bash
# Core integration package
dotnet add package AwsLambda.Host.OpenTelemetry

# Common packages for OTLP export
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add package OpenTelemetry.Extensions.Hosting

# Packages for X-Ray integration
dotnet add package OpenTelemetry.Contrib.Extensions.AWSXRay
```

### MVP Code Example

This example demonstrates how to add basic OpenTelemetry instrumentation to your Lambda function.

```csharp title="Program.cs" linenums="1"
using AwsLambda.Host.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Instrumentation.AWSLambda;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = LambdaApplication.CreateBuilder();

builder
    .Services.AddOpenTelemetry()// (1)!
    .WithTracing(tracing =>
    {
        tracing.AddAWSLambdaConfigurations();// (2)!
        tracing.AddSource("MyService");// (3)!
        tracing.SetResourceBuilder(
            ResourceBuilder.CreateDefault().AddService("MyService", serviceVersion: "1.0.0")
        );
        tracing.AddConsoleExporter();// (4)!
    });

await using var lambda = builder.Build();

lambda.UseOpenTelemetryTracing();// (5)!

lambda.OnShutdownFlushOpenTelemetry();// (6)!

lambda.MapHandler(// (7)!
    async ([Event] Request request, ILogger<Program> logger, CancellationToken cancellationToken) =>
    {
        logger.LogInformation("Responding to {Name}", request.Name);

        await Task.Delay(100, cancellationToken); // Simulate work

        return new Response($"Hello {request.Name}!");
    }
);

await lambda.RunAsync();

internal record Request(string Name);

internal record Response(string Message);
```

1. Add OpenTelemetry tracing to the DI container
2. Enable AWS Lambda configurations
3. Add a custom source for tracing
4. Export traces to a collector, in this case the console
5. Enable OpenTelemetry tracing in the Lambda host through middleware.
6. (Optional) Flush the OpenTelemetry traces at the end of the Lambda execution.
7. Write your Lambda handler like normal.

!!! tip
    OpenTelemetry tracing can be configured in multiple ways, including manually creating a trace provider using the [OpenTelemetry](https://www.nuget.org/packages/OpenTelemetry), or through registering OpenTelemetry services in your DI container using [OpenTelemetry.Extensions.Hosting](https://www.nuget.org/packages/OpenTelemetry.Extensions.Hosting). 

     When working with  `AwsLambda.Host`, its recommended to the latter approach and as such, this documentation focuses on it.

---

## How It Works: Source Generation and Interceptors

`AwsLambda.Host` relies on C# source generators to avoid runtime reflection and improve performance. The `UseOpenTelemetryTracing()` method is a key part of this system.

Here's the step-by-step process:

1. The `UseOpenTelemetryTracing()` method itself is an empty placeholder.
2. At compile time, a source generator finds all calls to this method.
3. For each call it finds, it emits a C# 12 Interceptor. This interceptor replaces the empty method call with code that adds a middleware delegate to the Lambda invocation pipeline.
4. This generated middleware calls an adapter in the `AwsLambda.Host.OpenTelemetry` package, which in turn uses the official `OpenTelemetry.Instrumentation.AWSLambda` package to wrap the Lambda handler execution in a root trace span.

This entire process happens during compilation, resulting in highly optimized code that instruments your handler without any reflection overhead at runtime.

Similarly, `OnShutdownFlushOpenTelemetry()` is an interceptor that registers a shutdown hook. This hook flushes the OpenTelemetry providers, ensuring buffered telemetry is sent before the Lambda execution environment terminates.

---

## Working With `AwsLambda.Host.OpenTelemetry`

### Configuration

Configuration is done using the standard OpenTelemetry .NET SDK extension methods on `IServiceCollection`. Official documentation for these methods can be found [here](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Extensions.Hosting/README.md).

`AwsLambda.Host.OpenTelemetry` expectes for an instance of `TracerProvider` to be registered in the DI container as this provider is used by `OpenTelemetry.Instrumentation.AWSLambda`. As such, it is your responsibility to configure the OpenTelemetry provider and ensure it is registered in the DI container. If it is not, an exception will be thrown at startup.

### Instrumenting The Invocation Pipeline 


### Gracefully Shutting & Cleaning Up

The OpenTelemetry `TracerProvider` and `MeterProvider` services both implement `IDisposable`. When the dependency injection container is disposed of during a normal application shutdown, it should trigger these providers to automatically flush any buffered telemetry. However, in a serverless environment where the lifecycle can be abrupt, this disposal is not always guaranteed to complete before the execution environment is frozen.

For situations where you notice data being dropped, or if you want to guarantee a flush attempt is made, `AwsLambda.Host.OpenTelemetry` provides the following explicit helper methods. They register a function during the application's shutdown phase to manually force-flush pending telemetry.

The following methods are available to be called on the `LambdaApplication` instance:

| Method                             | Description                                                                                                                                  |
| ---------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| `OnShutdownFlushOpenTelemetry()`   | A convenience method that registers shutdown hooks to flush both traces and metrics. It calls both `OnShutdownFlushTracer` and `OnShutdownFlushMeter` internally. This is the recommended method for most users. |
| `OnShutdownFlushTracer()`          | Registers a shutdown hook to force-flush only the `TracerProvider`. Use this if you are only tracing and not collecting metrics, or if you need separate control over flushing traces. |
| `OnShutdownFlushMeter()`           | Registers a shutdown hook to force-flush only the `MeterProvider`. Use this if you are only collecting metrics and not tracing.            |

For most applications, calling `lambda.OnShutdownFlushOpenTelemetry()` is sufficient to ensure all telemetry is flushed. If your application only uses tracing or metrics, but not both, you can use the more specific methods for clarity.

All three methods also accept an optional `timeoutMilliseconds` parameter. This allows you to specify a maximum duration for the flush operation. Importantly, these flush operations are non-blocking and respect the provided `CancellationToken`, ensuring they can gracefully exit if the Lambda execution environment signals a shutdown before the timeout elapses. This combined approach offers robust control over the flush duration within the limited time available during a Lambda shutdown.

!!! Warning
    When shutting down, Lambda only allocates up to 500ms of time for the execution environment to shut down. As such, it is important to make sure that shutdown logic such as flushing traces is executed as quickly as possible. More information about the Lambda execution environment lifecycle can be found [here](https://docs.aws.amazon.com/lambda/latest/dg/lambda-runtime-environment.html).

---

## Manual Instrumentation

`AwsLambda.Host.OpenTelemetry` helps you instrement your Lambda handlers with [OpenTelemetry.Instrumentation.AWSLambda](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AWSLambda), but to get the most out of observability, you should add custom instrumentation to your application code. In this section we cover how this can be done easily with the Dependancy Injection support provided by `AwsLambda.Host`.

!!! note
    This code is not specific to `AwsLambda.Host.OpenTelemetry` and follows the guidlines provided by Microsoft's [.NET distributed tracing documetation](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing).

A full working example of an instrumented Lambda application can be found in [here](../../examples/AwsLambda.Host.Example.OpenTelemetry)

### Custom Instrumentation Class

```csharp title="Instrumentation.cs" linenums="1"
using System.Diagnostics;

namespace AwsLambda.Host.Example.OpenTelemetry;

/// <summary>
///     It is recommended to use a custom type to hold references for ActivitySource. This avoids
///     possible type collisions with other components in the DI container.
/// </summary>
internal class Instrumentation : IDisposable
{
    internal const string ActivitySourceName = "MyLambda";
    internal const string ActivitySourceVersion = "1.0.0";

    internal ActivitySource ActivitySource { get; } =
        new(ActivitySourceName, ActivitySourceVersion);

    public void Dispose() => ActivitySource.Dispose();
}
```

### Custom Metrics Class

```csharp title="NameMetrics.cs" linenums="1"
using System.Diagnostics.Metrics;

namespace AwsLambda.Host.Example.OpenTelemetry;

public class NameMetrics
{
    private readonly Counter<int> _namesProcessed;

    public NameMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("MyLambda.Service");
        _namesProcessed = meter.CreateCounter<int>("MyLambda.Service.Processed");
    }

    public void ProcessName(string name) =>
        _namesProcessed.Add(1, new KeyValuePair<string, object?>("name", name));
}
```


### Instrument A Service

```csharp title="NameService.cs" linenums="1"
using System.Diagnostics;

public class NameService
{
    private static readonly ActivitySource Source = new("MyApplication.NameService");

    public string GetFullName(string name)
    {
        using var activity = Source.StartActivity("GetFullName");
        activity?.SetTag("input.name", name);

        // ... some work ...

        var fullName = $"{name} Smith";
        activity?.SetTag("output.fullname", fullName);

        return fullName;
    }
}
```

### Instrument A Handler

```csharp title="Function.cs" linenums="1"
using AwsLambda.Host.Builder;

namespace AwsLambda.Host.Example.OpenTelemetry;

internal static class Function
{
    internal static async Task<Response> Handler(
        [Event] Request request,
        IService service,
        Instrumentation instrumentation,
        CancellationToken cancellationToken
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity();

        var message = await service.GetMessage(request.Name, cancellationToken);

        return new Response(message, DateTime.UtcNow);
    }
}
```
