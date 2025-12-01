# OpenTelemetry

## What is OpenTelemetry Integration?

The `AwsLambda.Host.OpenTelemetry` package provides seamless integration with the official [OpenTelemetry.Instrumentation.AWSLambda](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AWSLambda) package. It acts as a smart adapter layer for `AwsLambda.Host`, using **C# 12 interceptors and source generation** to automatically instrument your Lambda handlers with minimal overhead.

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

## Configuration

Configuration is done using the standard OpenTelemetry .NET SDK extension methods on `IServiceCollection`. Official documentation for these methods can be found [here](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Extensions.Hosting/README.md).

---

## Custom Instrumentation

`AwsLambda.Host.OpenTelemetry` helps you instrement your Lambda handlers with [OpenTelemetry.Instrumentation.AWSLambda](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AWSLambda), but to get the most out of observability, you should add custom instrumentation to your application code. In this section we cover how this can be done. 

!!! note
    This code is not specific to `AwsLambda.Host.OpenTelemetry` and follows the guidlines provided by Microsoft's [.NET distributed tracing documetation](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing).

### Custom Spans (Activities)

Inject an `ActivitySource` to create custom spans that represent specific units of work, like a database call or an API request.

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

### Custom Metrics

Inject a `Meter` and create `Counter`, `Histogram`, or `UpDownCounter` instruments to record business or performance metrics.

```csharp title="OrderProcessor.cs" linenums="1"
using System.Diagnostics.Metrics;

public class OrderProcessor
{
    private static readonly Meter Meter = new("MyApplication.Metrics");
    private static readonly Counter<int> OrdersProcessed = Meter.CreateCounter<int>("orders.processed");
    private static readonly Histogram<double> OrderValue = Meter.CreateHistogram<double>("orders.value");

    public void Process(Order order)
    {
        // ... process order ...

        OrdersProcessed.Add(1);
        OrderValue.Record(order.Value);
    }
}
```

---

## Viewing Traces: A Practical Example

You can run the included example project to see tracing in action with a local Jaeger instance.

### Dependencies

- [Docker](https://docs.docker.com/get-docker/) & [Docker Compose](https://docs.docker.com/compose/install/)

### 1. Start Jaeger

Navigate to the example directory and start the Jaeger container.

```bash
cd ./examples/AwsLambda.Host.Example.OpenTelemetry
docker compose up
```

Jaeger UI will be available at [`http://localhost:16686`](http://localhost:16686).

### 2. Run the Lambda Function

In a new terminal, run the Lambda function. It is configured to export traces to the Jaeger instance started above.

```bash
# In ./examples/AwsLambda.Host.Example.OpenTelemetry
dotnet run
```

### 3. Invoke the Function

You can use any tool to send a POST request to `http://localhost:8080`, which is the default for `dotnet run`.

Using `curl`:
```bash
curl -X POST "http://localhost:8080" \
-H "Content-Type: application/json" \
-d '{"Name": "World"}'
```

### 4. View the Trace

Refresh the Jaeger UI. You should see a new trace for the service. Clicking on it will reveal the full trace, including the handler invocation span and any custom spans you created.

!!! warning "Traces Not Appearing?"
    It may take a few seconds for traces to be exported. If they don't appear, stop the running Lambda function (`Ctrl+C`). This triggers the shutdown hook, which forces a flush of any buffered telemetry.
