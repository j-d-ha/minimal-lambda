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

# Official AWS Lambda Instrumentation
dotnet add package OpenTelemetry.Instrumentation.AWSLambda

# Common packages for OTLP export
dotnet add package OpenTelemetry.Exporter.Otlp
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

// 1. Add OpenTelemetry tracing to the DI container
builder
    .Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        // Enable AWS Lambda configurations
        tracing.AddAWSLambdaConfigurations();
        // Add a custom source for tracing
        tracing.AddSource("MyService");
        tracing.SetResourceBuilder(
            ResourceBuilder.CreateDefault().AddService("MyService", serviceVersion: "1.0.0")
        );
        // Export traces to the console for debugging
        tracing.AddConsoleExporter();
    });

await using var lambda = builder.Build();

// 2. Enable OpenTelemetry tracing in the Lambda host through middleware.
lambda.UseOpenTelemetryTracing();

// 3. (Optional) Flush the OpenTelemetry traces at the end of the Lambda execution.
lambda.OnShutdownFlushOpenTelemetry();

// 4. Write your Lambda handler like normal.
lambda.MapHandler(
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

---

## How It Works: Compile-Time Magic

The `UseOpenTelemetryTracing()` method is the key to this integration. While it looks like a simple method call, it's actually an empty stub that acts as a hook for the `AwsLambda.Host` source generator.

At compile time, the generator finds this call and **intercepts** it. It then generates code that wraps your handler delegate inside the root tracing logic provided by `OpenTelemetry.Instrumentation.AWSLambda`. This means your handler is automatically timed and traced with no runtime performance penalty from reflection.

The `OnShutdownFlushOpenTelemetry()` method registers a lifecycle hook that gracefully flushes the OpenTelemetry `TracerProvider` and `MeterProvider`, ensuring all buffered telemetry is sent before the Lambda terminates.

---

## Configuration

Configuration is done using the standard OpenTelemetry .NET SDK extension methods on `IServiceCollection`.

### Basic Configuration (OTLP Exporter)

The most common scenario is exporting telemetry to an observability platform via the OpenTelemetry Protocol (OTLP).

```csharp title="Program.cs" linenums="1"
var lambda = LambdaApplication.Create();

lambda.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAWSLambdaConfigurations() // Adds Lambda-specific resource attributes
        .AddOtlpExporter()) // Exports traces via OTLP
    .WithMetrics(metrics => metrics
        .AddAWSLambdaConfigurations() // Adds Lambda-specific resource attributes
        .AddOtlpExporter()); // Exports metrics via OTLP

lambda.UseOpenTelemetryTracing();
lambda.OnShutdownFlushOpenTelemetry();

var app = lambda.Build();
// ...
```

!!! tip "OTLP Endpoint Configuration"
    The OTLP exporter endpoint is configured via the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable.

### AWS X-Ray Integration

To integrate with AWS X-Ray, use the AWS-provided instrumentation and propagator.

```csharp title="Program.cs" linenums="1"
using OpenTelemetry.Trace;
using OpenTelemetry.Contrib.Extensions.AWSXRay; // AWS X-Ray Propagator

// ...

lambda.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAWSLambdaConfigurations()
            .AddAWSXRayTraceId() // Use X-Ray format for Trace IDs
            .AddOtlpExporter();

        // Propagate X-Ray context
        Sdk.SetDefaultTextMapPropagator(new AWSXRayPropagator());
    });

lambda.UseOpenTelemetryTracing();
lambda.OnShutdownFlushOpenTelemetry();
// ...
```

---

## Custom Instrumentation

To get the most out of observability, you should add custom instrumentation to your application code.

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
