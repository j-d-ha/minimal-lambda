# OpenTelemetry

**What is OpenTelemetry Integration?**

The `AwsLambda.Host.OpenTelemetry` package provides comprehensive observability for your serverless application by integrating distributed tracing and metrics collection directly into the AWS Lambda lifecycle. It builds on the vendor-neutral OpenTelemetry SDK, allowing you to export telemetry data to any compatible observability backend.

---

## Key Benefits

- **Automatic Instrumentation**: Automatically creates trace spans for the entire Lambda lifecycle, including `OnInit` (cold starts), handler invocation, and `OnShutdown`.
- **Distributed Tracing**: Propagates trace context across services, giving you end-to-end visibility in a microservices architecture.
- **Metrics Collection**: Capture and export performance and business metrics.
- **Lifecycle Integration**: Seamlessly hooks into the `AwsLambda.Host` lifecycle for accurate cold start detection and telemetry flushing.
- **Vendor-Neutral**: Compatible with any OpenTelemetry-compliant backend, including Jaeger, Datadog, New Relic, Honeycomb, and AWS X-Ray.
- **Custom Instrumentation**: Easily create custom spans and metrics to capture application-specific logic.

---

## Quick Start

This example demonstrates how to add basic OpenTelemetry instrumentation. With just a few lines of code, you get automatic tracing for your Lambda handler.

```csharp title="Program.cs" linenums="1"
using System.Diagnostics;
using AwsLambda.Host.Builder;
using AwsLambda.Host.OpenTelemetry;
using OpenTelemetry.Trace;

var builder = LambdaApplication.CreateBuilder();

// 1. Add and configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("MyCustomSource") // Add a custom source for tracing
        .AddConsoleExporter()); // Export traces to the console for debugging

// 2. Add the Lambda-specific OpenTelemetry instrumentation
builder.Services.AddLambdaOpenTelemetry();

var lambda = builder.Build();

// 3. The handler invocation will be automatically traced
lambda.MapHandler(([Event] Request request, ILogger<Program> logger) =>
{
    // 4. (Optional) Create a custom activity span
    using var activity = new ActivitySource("MyCustomSource").StartActivity("ProcessingRequest");
    activity?.SetTag("name", request.Name);

    logger.LogInformation("Responding to {Name}", request.Name);
    return new Response($"Hello {request.Name}!");
});

await lambda.RunAsync();

internal record Request(string Name);
internal record Response(string Message);
```

---

## Configuration

Configuration is done using the standard OpenTelemetry .NET SDK extension methods on `IServiceCollection`. The `AddLambdaOpenTelemetry()` call then wires up the instrumentation with the Lambda lifecycle.

### Basic Configuration (OTLP Exporter)

The most common scenario is exporting telemetry to an observability platform via the OpenTelemetry Protocol (OTLP).

```csharp title="Program.cs" linenums="1"
var builder = LambdaApplication.CreateBuilder();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAWSLambdaConfigurations() // Adds Lambda-specific resource attributes
        .AddOtlpExporter()) // Exports traces via OTLP
    .WithMetrics(metrics => metrics
        .AddAWSLambdaConfigurations() // Adds Lambda-specific resource attributes
        .AddOtlpExporter()); // Exports metrics via OTLP

builder.Services.AddLambdaOpenTelemetry();

var lambda = builder.Build();
// ...
```

!!! tip "OTLP Endpoint Configuration"
    The OTLP exporter endpoint is configured via the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable.

### Configuring Tracing

You can configure tracing providers, add custom sources, and define sampling rules.

```csharp title="Program.cs" linenums="1"
using OpenTelemetry.Trace;

// ...

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAWSLambdaConfigurations()
        .AddSource("MyApplication.*") // Listen to custom ActivitySources
        .SetSampler(new AlwaysOnSampler()) // Configure sampling
        .AddOtlpExporter());
```

### Configuring Metrics

Configure metrics providers and add custom meters to record application-specific measurements.

```csharp title="Program.cs" linenums="1"
using OpenTelemetry.Metrics;

// ...

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddAWSLambdaConfigurations()
        .AddMeter("MyApplication.Metrics") // Listen to custom Meters
        .AddPrometheusExporter()); // Or any other exporter
```

### AWS X-Ray Integration

To integrate with AWS X-Ray, use the AWS-provided instrumentation and propagator.

```csharp title="Program.cs" linenums="1"
using OpenTelemetry.Trace;

// ...

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAWSLambdaConfigurations()
            .AddAWSXRayTraceId() // Use X-Ray format for Trace IDs
            .AddOtlpExporter();

        // Propagate X-Ray context
        Sdk.SetDefaultTextMapPropagator(new AWSXRayPropagator());
    });
```

---

## Lifecycle Integration

`AwsLambda.Host.OpenTelemetry` automatically instruments the key phases of the Lambda execution lifecycle.

- **`OnInit`**: The application startup phase (`OnInitAsync`) is wrapped in its own trace span. This allows you to precisely measure cold start times by analyzing the duration of the `OnInit` span.

- **Invocation**: Each handler invocation is automatically traced. The trace captures the full execution time, including any middleware in the pipeline.

- **`OnShutdown`**: The `OnShutdownAsync` phase is traced. Importantly, the OpenTelemetry `TracerProvider` is flushed during this phase, ensuring that all buffered telemetry is sent before the execution environment is frozen.

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
- [AWS Lambda Test Tool](https://github.com/aws/aws-lambda-dotnet/tree/master/Tools/LambdaTestTool-v2)

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

You can use the AWS Lambda Test Tool to invoke the function locally. If you don't have it running, start it from the example directory:

```bash
dotnet lambda-test-tool
```

In the Test Tool UI:
1. Select the `AwsLambda.Host.Example.OpenTelemetry` function.
2. Choose the saved "SayHello" example request from the "Example Requests" dropdown.
3. Click **"Execute"**.

### 4. View the Trace

Refresh the Jaeger UI. You should see a new trace for the `AwsLambda.Host.Example.OpenTelemetry` service. Clicking on it will reveal the full trace, including the `OnInit` span and the handler invocation span.

!!! warning "Traces Not Appearing?"
    It may take a few seconds for traces to be exported. If they don't appear, stop the running Lambda function (`Ctrl+C`). This triggers the `OnShutdown` hook, which forces a flush of any buffered telemetry.

---

## Installation

Install the OpenTelemetry integration package and any required exporter packages.

```bash
# Core integration package
dotnet add package AwsLambda.Host.OpenTelemetry

# Common packages for OTLP export
dotnet add package OpenTelemetry.Exporter.Otlp
dotnet add package OpenTelemetry.Extensions.Hosting

# Packages for X-Ray integration
dotnet add package OpenTelemetry.Contrib.Extensions.AWSXRay
dotnet add package OpenTelemetry.Contrib.Instrumentation.AWS
```