# OpenTelemetry

**What is OpenTelemetry Integration?**

The `AwsLambda.Host.OpenTelemetry` package provides comprehensive observability integration for distributed tracing and metrics collection in AWS Lambda functions.

**Capabilities**:

- **Distributed Tracing** - Automatic span creation and context propagation for Lambda invocations
- **Metrics Collection** - Performance and business metrics exportable to standard observability backends
- **AWS Lambda Instrumentation** - Lambda-specific insights including cold starts, warm invocations, and error tracking
- **Lifecycle Integration** - Seamless integration with OnInit, Invocation, and OnShutdown phases
- **Vendor-Neutral** - Built on the OpenTelemetry SDK for compatibility with any observability backend

**Supported Exporters**:

- OTLP (OpenTelemetry Protocol)
- Jaeger
- AWS X-Ray
- Datadog
- New Relic
- CloudWatch
- And more...

!!! info "Learn More About OpenTelemetry"
    - **[OpenTelemetry Integration Guide](opentelemetry.md)** - Complete setup and configuration

---

## Quick Start

### Using OpenTelemetry

```csharp title="Program.cs" linenums="1"
using AwsLambda.Host.Builder;
using AwsLambda.Host.OpenTelemetry;

var builder = LambdaApplication.CreateBuilder();

// Add OpenTelemetry tracing and metrics
builder.Services.AddLambdaOpenTelemetry();

var lambda = builder.Build();

lambda.MapHandler(([Event] Request request) =>
{
    // Automatic span creation for this invocation
    return new Response($"Hello {request.Name}!");
});

await lambda.RunAsync();

internal record Request(string Name);
internal record Response(string Message);
```

---

## Choosing the Right Feature

### When to Use OpenTelemetry

Use the OpenTelemetry package when:

- ✅ You need distributed tracing across microservices
- ✅ You want to monitor Lambda performance and cold starts
- ✅ You need to export metrics to observability platforms (Datadog, New Relic, etc.)
- ✅ You want automatic instrumentation for Lambda invocations
- ✅ You're building complex serverless architectures

---

## Installation

### OpenTelemetry Package 

```bash
dotnet add package AwsLambda.Host.OpenTelemetry
```