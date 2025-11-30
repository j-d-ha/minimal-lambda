# Build AWS Lambda Functions with .NET Hosting Patterns

[![Main Build](https://github.com/j-d-ha/aws-lambda-host/actions/workflows/main-build.yaml/badge.svg)](https://github.com/j-d-ha/aws-lambda-host/actions/workflows/main-build.yaml)
[![codecov](https://codecov.io/gh/j-d-ha/aws-lambda-host/graph/badge.svg?token=BWORPTQ0UK)](https://codecov.io/gh/j-d-ha/aws-lambda-host)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=j-d-ha_aws-lambda-host&metric=alert_status&token=9fb519975d91379dcfbc6c13a4bd4207131af6e3)](https://sonarcloud.io/summary/new_code?id=j-d-ha_aws-lambda-host)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/j-d-ha/aws-lambda-host/blob/main/LICENSE)

A modern .NET framework that brings familiar .NET Core patterns to AWS Lambda - middleware, dependency injection, lifecycle hooks, and async-first design.

[Get Started](getting-started/){ .md-button .md-button--primary }
[View Examples](examples/){ .md-button }

---

## Why AwsLambda.Host?

Stop writing boilerplate Lambda code. Start building features with patterns you already know.

=== "Traditional Lambda"

    ```csharp
    // ❌ Manual DI container setup
    var services = new ServiceCollection();
    services.AddScoped<IGreetingService, GreetingService>();

    await using var rootProvider = services.BuildServiceProvider();

    await LambdaBootstrapBuilder
        .Create<APIGatewayProxyRequest, APIGatewayProxyResponse>(
            async (request, context) =>
            {
                // ❌ Manual scope creation for each invocation
                using var scope = rootProvider.CreateScope();

                // ❌ Manual service resolution from scope
                var service = scope.ServiceProvider.GetRequiredService<IGreetingService>();

                // ❌ Manual cancellation token creation from context
                using var cts = new CancellationTokenSource(
                    context.RemainingTime - TimeSpan.FromMilliseconds(500)
                );

                // ❌ Manual JSON deserialization
                var requestContent = JsonSerializer.Deserialize<GreetingRequest>(request.Body);

                var greeting = await service.GreetAsync(requestContent.Name, cts.Token);

                // ❌ Manual JSON serialization
                var responseJson = JsonSerializer.Serialize(
                    new GreetingResponse(greeting, DateTime.UtcNow)
                );

                return new APIGatewayProxyResponse
                {
                    Body = responseJson,
                    StatusCode = 200,
                    Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" },
                };
            },
            new DefaultLambdaJsonSerializer()
        )
        .Build()
        .RunAsync();

    ```

=== "aws-lambda-host"

    ```csharp
    // ✅ Familiar .NET Core builder pattern
    var builder = LambdaApplication.CreateBuilder();

    builder.Services.AddScoped<IGreetingService, GreetingService>();

    await using var lambda = builder.Build();

    lambda.MapHandler(
        async (
            // ✅ Automatic envelope deserialization with strong typing
            [Event] ApiGatewayRequestEnvelope<GreetingRequest> request,
            // ✅ Automatic DI injection - proper scoped lifetime per invocation
            IGreetingService service,
            // ✅ Automatic cancellation token - framework manages timeout
            CancellationToken cancellationToken
        ) =>
        {
            var greeting = await service.GreetAsync(request.BodyContent.Name, cancellationToken);

            // ✅ Type-safe response - automatic JSON serialization
            return new ApiGatewayResponseEnvelope<GreetingResponse>
            {
                BodyContent = new GreetingResponse(greeting, DateTime.UtcNow),
                StatusCode = 200,
                Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" },
            };
        }
    );

    await lambda.RunAsync();
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

| Package | Description | NuGet | Downloads |
|---------|-------------|-------|-----------|
| [**AwsLambda.Host.Envelopes.Sqs**](features/envelopes/sqs.md) | Simple Queue Service events with typed message bodies | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs/) |
| [**AwsLambda.Host.Envelopes.Sns**](features/envelopes/sns.md) | Simple Notification Service messages | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns/) |
| [**AwsLambda.Host.Envelopes.ApiGateway**](features/envelopes/api-gateway.md) | REST, HTTP, and WebSocket APIs | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway/) |
| [**AwsLambda.Host.Envelopes.Kinesis**](features/envelopes/kinesis.md) | Data Streams with typed records | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kinesis) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kinesis/) |
| [**AwsLambda.Host.Envelopes.KinesisFirehose**](features/envelopes/kinesis-firehose.md) | Data transformation | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.KinesisFirehose) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.KinesisFirehose/) |
| [**AwsLambda.Host.Envelopes.Kafka**](features/envelopes/kafka.md) | MSK and self-managed Kafka | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Kafka.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kafka) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Kafka.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kafka/) |
| [**AwsLambda.Host.Envelopes.CloudWatchLogs**](features/envelopes/cloudwatch-logs.md) | Log subscriptions | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.CloudWatchLogs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.CloudWatchLogs) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.CloudWatchLogs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.CloudWatchLogs/) |
| [**AwsLambda.Host.Envelopes.Alb**](features/envelopes/alb.md) | Application Load Balancer requests | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb/) |

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
