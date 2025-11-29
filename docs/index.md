# Build AWS Lambda Functions with .NET Hosting Patterns

[![Main Build](https://github.com/j-d-ha/aws-lambda-host/actions/workflows/main-build.yaml/badge.svg)](https://github.com/j-d-ha/aws-lambda-host/actions/workflows/main-build.yaml)
[![codecov](https://codecov.io/gh/j-d-ha/aws-lambda-host/graph/badge.svg?token=BWORPTQ0UK)](https://codecov.io/gh/j-d-ha/aws-lambda-host)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=j-d-ha_aws-lambda-host&metric=alert_status&token=9fb519975d91379dcfbc6c13a4bd4207131af6e3)](https://sonarcloud.io/summary/new_code?id=j-d-ha_aws-lambda-host)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/j-d-ha/aws-lambda-host/blob/main/LICENSE)

A modern .NET framework that brings familiar ASP.NET Core patterns to AWS Lambda - middleware, dependency injection, and async-first design.

[Get Started](getting-started/){ .md-button .md-button--primary }
[View Examples](examples/){ .md-button }

---

## Why aws-lambda-host?

Stop writing boilerplate Lambda code. Start building features with patterns you already know.

=== "Traditional Lambda"

    ```csharp
    using Amazon.Lambda.RuntimeSupport;
    using Amazon.Lambda.Serialization.SystemTextJson;
    using Microsoft.Extensions.DependencyInjection;
    
    // Manual DI container setup - must be done ONCE at startup
    var services = new ServiceCollection();
    services.AddScoped<IGreetingService, GreetingService>();
    var rootProvider = services.BuildServiceProvider();
    
    // Capture service provider outside handler
    // ⚠️ Problem: Can't create scopes per invocation easily
    var service = rootProvider.GetRequiredService<IGreetingService>();
    
    // Manual bootstrap initialization
    await LambdaBootstrapBuilder
        .Create<GreetingRequest, GreetingResponse>(
            async (request, context) =>
            {
                // ⚠️ Manual cancellation token creation from context
                using var cts = new CancellationTokenSource(
                    context.RemainingTime - TimeSpan.FromMilliseconds(500)
                );
    
                // ⚠️ Using singleton-scoped service for all invocations
                // No proper scoped lifetime per invocation!
                var greeting = await service.GreetAsync(request.Name, cts.Token);
    
                return new GreetingResponse(greeting, DateTime.UtcNow);
            },
            new DefaultLambdaJsonSerializer()
        )
        .Build()
        .RunAsync();
    
    // Models
    public record GreetingRequest(string Name);
    
    public record GreetingResponse(string Message, DateTime Timestamp);
    
    // Service interface and implementation
    public interface IGreetingService
    {
        Task<string> GreetAsync(string name, CancellationToken cancellationToken);
    }
    
    public class GreetingService : IGreetingService
    {
        public async Task<string> GreetAsync(string name, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken); // Simulate async work
            return $"Hello {name}!";
        }
    }
    ```

=== "aws-lambda-host"

    ```csharp
    using AwsLambda.Host.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    
    var builder = LambdaApplication.CreateBuilder();
    
    // Register services with DI
    builder.Services.AddScoped<IGreetingService, GreetingService>();
    
    var lambda = builder.Build();
    
    // ✅ Clean handler with automatic DI and cancellation token injection
    lambda.MapHandler(
        async (
            [Event] GreetingRequest request,
            IGreetingService service,
            CancellationToken cancellationToken
        ) =>
        {
            // ✅ Cancellation token automatically provided by the framework
            var greeting = await service.GreetAsync(request.Name, cancellationToken);
            return new GreetingResponse(greeting, DateTime.UtcNow);
        }
    );
    
    await lambda.RunAsync();
    
    // Models
    public record GreetingRequest(string Name);
    
    public record GreetingResponse(string Message, DateTime Timestamp);
    
    // Service interface and implementation
    public interface IGreetingService
    {
        Task<string> GreetAsync(string name, CancellationToken cancellationToken);
    }
    
    public class GreetingService : IGreetingService
    {
        public async Task<string> GreetAsync(string name, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken); // Simulate async work
            return $"Hello {name}!";
        }
    }
    ```

---

## Key Features

### :material-view-dashboard-outline: .NET Hosting Patterns

Use middleware, builder pattern, and dependency injection similar to ASP.NET Core, with proper scoped lifetime management per invocation.

[Learn about DI](guides/dependency-injection.md){ .md-button }

### :material-lightning-bolt-outline: Async-First Design

Native support for async/await with proper Lambda timeout and cancellation handling built-in.

[See lifecycle management](guides/lifecycle-management.md){ .md-button }

### :material-code-braces: Source Generators & Interceptors

Compile-time code generation and method interception for optimal performance with zero runtime reflection.

[Explore advanced topics](advanced/source-generators.md){ .md-button }

### :material-rocket-launch-outline: AOT Ready

Full support for Ahead-of-Time compilation for faster cold starts and reduced memory footprint.

[AOT compilation guide](advanced/aot-compilation.md){ .md-button }

### :material-chart-line: Built-in Observability

OpenTelemetry integration for distributed tracing with automatic root span creation and custom instrumentation.

[OpenTelemetry setup](features/opentelemetry.md){ .md-button }

### :material-code-json: Flexible Handler Registration

Simple, declarative API for mapping Lambda event types to handlers with compile-time type safety.

[Handler registration](guides/handler-registration.md){ .md-button }

### :material-speedometer: Minimal Runtime Overhead

No unnecessary abstractions - efficient use of Lambda resources with optimized execution paths.

[Performance optimization](advanced/performance-optimization.md){ .md-button }

---

## Quick Start

Install the NuGet package:

```bash
dotnet add package AwsLambda.Host
```

Create your first Lambda handler:

```csharp
using AwsLambda.Host.Builder;
using Microsoft.Extensions.DependencyInjection;

// Create the application builder
var builder = LambdaApplication.CreateBuilder();

// Register your services
builder.Services.AddScoped<IGreetingService, GreetingService>();

// Build the Lambda application
var lambda = builder.Build();

// Map your handler - services are automatically injected
lambda.MapHandler(([Event] string input, IGreetingService greeting)
    => greeting.Greet(input));

// Run the Lambda
await lambda.RunAsync();

// Define your service
public interface IGreetingService
{
    string Greet(string name);
}

public class GreetingService : IGreetingService
{
    public string Greet(string name) => $"Hello {name}!";
}
```

!!! tip "Next Steps"
    Ready to dive deeper? Check out the [Getting Started Guide](getting-started/) for a complete tutorial, or explore the [Examples](examples/) to see real-world applications.

---

## Packages

### Core Packages

The core packages provide the fundamental hosting framework, abstractions, and observability support for building AWS Lambda functions.

| Package | Description | NuGet | Downloads |
|---------|-------------|-------|-----------|
| [**AwsLambda.Host**](api-reference/host.md) | Core hosting framework with middleware and DI | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.svg)](https://www.nuget.org/packages/AwsLambda.Host) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.svg)](https://www.nuget.org/packages/AwsLambda.Host/) |
| [**AwsLambda.Host.Abstractions**](api-reference/abstractions.md) | Core interfaces and contracts | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Abstractions.svg)](https://www.nuget.org/packages/AwsLambda.Host.Abstractions) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Abstractions.svg)](https://www.nuget.org/packages/AwsLambda.Host.Abstractions/) |
| [**AwsLambda.Host.OpenTelemetry**](features/opentelemetry.md) | Distributed tracing and observability | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.OpenTelemetry.svg)](https://www.nuget.org/packages/AwsLambda.Host.OpenTelemetry) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.OpenTelemetry.svg)](https://www.nuget.org/packages/AwsLambda.Host.OpenTelemetry/) |

### Envelope Packages

Envelope packages provide type-safe handling of AWS Lambda event sources with automatic payload deserialization.

!!! info "What are Envelopes?"
    Envelopes wrap AWS Lambda events with strongly-typed payload handling, giving you compile-time type safety and automatic deserialization of message bodies from SQS, SNS, Kinesis, and other event sources.

    [Learn more about envelopes](features/envelopes/){ .md-button }

Available envelope packages:

- [**SQS**](features/envelopes/sqs.md) - Simple Queue Service events with typed message bodies
- [**SNS**](features/envelopes/sns.md) - Simple Notification Service messages
- [**API Gateway**](features/envelopes/api-gateway.md) - REST, HTTP, and WebSocket APIs
- [**Kinesis**](features/envelopes/kinesis.md) - Data Streams with typed records
- [**Kinesis Firehose**](features/envelopes/kinesis-firehose.md) - Data transformation
- [**Kafka**](features/envelopes/kafka.md) - MSK and self-managed Kafka
- [**CloudWatch Logs**](features/envelopes/cloudwatch-logs.md) - Log subscriptions
- [**ALB**](features/envelopes/alb.md) - Application Load Balancer requests

[Browse all envelope packages](features/envelopes/){ .md-button }

---

## Examples & Use Cases

Explore complete example projects demonstrating real-world Lambda patterns:

- **[Hello World](examples/hello-world.md)** - Basic Lambda with dependency injection and middleware
- **[REST API](examples/api-rest.md)** - API Gateway integration with request/response handling
- **[SQS Processing](examples/sqs-processing.md)** - Event-driven message processing
- **[OpenTelemetry](examples/opentelemetry-example.md)** - Full observability with distributed tracing
- **[AOT Compilation](examples/aot-example.md)** - Native AOT for optimal cold start performance

[View all examples](examples/){ .md-button }

---

## Community & Resources

### Get Involved

- **[GitHub Repository](https://github.com/j-d-ha/aws-lambda-host)** - Source code, issues, and discussions
- **[Changelog](resources/changelog.md)** - Version history and release notes
- **[License](https://github.com/j-d-ha/aws-lambda-host/blob/main/LICENSE)** - MIT License

### Documentation

- **[Getting Started](getting-started/)** - Installation and first Lambda tutorial
- **[Guides](guides/)** - Comprehensive feature documentation
- **[Features](features/)** - Envelopes and OpenTelemetry integration
- **[API Reference](api-reference/)** - Detailed API documentation
- **[Advanced Topics](advanced/)** - AOT, source generators, and performance

### Support

Need help or want to contribute?

- Browse the [FAQ](resources/faq.md) for common questions
- Check the [Troubleshooting Guide](resources/troubleshooting.md) for solutions
- Visit the [Community Page](resources/community.md) for support channels

---

**Ready to modernize your Lambda development?** [Get started now](getting-started/){ .md-button .md-button--primary }
