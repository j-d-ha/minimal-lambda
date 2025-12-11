# MinimalLambda

[![Main Build](https://github.com/j-d-ha/minimal-lambda/actions/workflows/main-build.yaml/badge.svg)](https://github.com/j-d-ha/minimal-lambda/actions/workflows/main-build.yaml)
[![codecov](https://codecov.io/gh/j-d-ha/minimal-lambda/graph/badge.svg?token=BWORPTQ0UK)](https://codecov.io/gh/j-d-ha/minimal-lambda)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=j-d-ha_minimal-lambda&metric=alert_status&token=9fb519975d91379dcfbc6c13a4bd4207131af6e3)](https://sonarcloud.io/summary/new_code?id=j-d-ha_minimal-lambda)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

**ASP.NET Core-style patterns for AWS Lambda** – Build Lambda functions (for any trigger) using the same clean, declarative patterns as ASP.NET Core Minimal APIs.

> 📚 **[View Full Documentation](https://j-d-ha.github.io/minimal-lambda/)**

## Overview

**If you know ASP.NET Core Minimal APIs, you already know MinimalLambda.**

MinimalLambda brings the clean, declarative coding style of ASP.NET Core Minimal APIs to AWS Lambda – for **any trigger type** (SQS, SNS, API Gateway, Kinesis, S3, EventBridge, and more):

- **Same builder pattern**: `LambdaApplication.CreateBuilder()` → `Build()` → `RunAsync()`
- **Same dependency injection**: `builder.Services.AddScoped<IMyService, MyService>()`
- **Same handler mapping**: `lambda.MapHandler(...)` just like `app.MapGet(...)`
- **Same middleware pipeline**: `lambda.Use(...)` for cross-cutting concerns
- **Same foundation**: Built on `Microsoft.Extensions.Hosting` like ASP.NET Core

Instead of writing boilerplate Lambda handlers, you get familiar .NET patterns with automatic dependency injection, proper scoped lifetimes, middleware support, and compile-time code generation for zero reflection overhead.

## Key Features

- **Minimal API Pattern** – Map handlers with `lambda.MapHandler(...)` just like `app.MapGet()` in ASP.NET Core – clean, declarative, and intuitive
- **Dependency Injection** – Constructor and parameter injection using `Microsoft.Extensions.DependencyInjection` with proper scoped lifetimes per invocation
- **Middleware Pipeline** – Familiar `Use()` pattern for cross-cutting concerns like logging, validation, and error handling
- **Source Generated** – Compile-time code generation for zero reflection overhead and optimal performance
- **AOT Ready** – Native AOT compilation support for sub-100ms cold starts
- **Built-in Observability** – OpenTelemetry integration for distributed tracing and metrics
- **Type-Safe Envelopes** – Strongly-typed event wrappers for SQS, SNS, API Gateway, Kinesis, and more
- **Minimal Runtime Overhead** – Lightweight abstraction layer built on the same foundation as ASP.NET Core

## From ASP.NET Core to Lambda

If you're familiar with ASP.NET Core Minimal APIs, the transition to MinimalLambda is seamless:

| ASP.NET Core Minimal API | MinimalLambda |
|---|---|
| `WebApplication.CreateBuilder()` | `LambdaApplication.CreateBuilder()` |
| `app.MapGet("/hello", ...)` | `lambda.MapHandler(...)` |
| `builder.Services.AddScoped<T>()` | `builder.Services.AddScoped<T>()` |
| `app.UseMiddleware<T>()` | `lambda.UseMiddleware<T>()` |
| `app.RunAsync()` | `lambda.RunAsync()` |
| `HttpContext` dependency injection | `ILambdaContext` dependency injection |
| Route parameters | `[Event]` attribute for event binding |

The patterns are intentionally identical – if you know one, you know the other.

## Packages

The framework is divided into multiple NuGet packages:

### Core Packages

The core packages provide the fundamental hosting framework, abstractions, and observability support
for building AWS Lambda functions.

| Package                                                                        | NuGet                                                                                                                                    | Downloads                                                                                                                                      |
|--------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------|
| [**MinimalLambda**](./src/MinimalLambda/README.md)                        | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.svg)](https://www.nuget.org/packages/MinimalLambda)                             | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.svg)](https://www.nuget.org/packages/MinimalLambda/)                             |
| [**MinimalLambda.Abstractions**](./src/MinimalLambda.Abstractions/README.md)   | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Abstractions.svg)](https://www.nuget.org/packages/MinimalLambda.Abstractions)   | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Abstractions.svg)](https://www.nuget.org/packages/MinimalLambda.Abstractions/)   |
| [**MinimalLambda.OpenTelemetry**](./src/MinimalLambda.OpenTelemetry/README.md) | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.OpenTelemetry.svg)](https://www.nuget.org/packages/MinimalLambda.OpenTelemetry) | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.OpenTelemetry.svg)](https://www.nuget.org/packages/MinimalLambda.OpenTelemetry/) |

### Envelopes Packages

Envelope packages provide specialized support for handling different AWS Lambda event sources,
including SQS, SNS, API Gateway, Kinesis, and more.

| Package                                                                                              | NuGet                                                                                                                                                            | Downloads                                                                                                                                                              |
|------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [**MinimalLambda.Envelopes.Sqs**](./MinimalLambda.Envelopes.Sqs/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Envelopes.Sqs.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Sqs)                         | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Envelopes.Sqs.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Sqs/)                         |
| [**MinimalLambda.Envelopes.ApiGateway**](./MinimalLambda.Envelopes.ApiGateway/README.md)           | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.ApiGateway)           | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.ApiGateway/)           |
| [**MinimalLambda.Envelopes.Sns**](./MinimalLambda.Envelopes.Sns/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Envelopes.Sns.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Sns)                         | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Envelopes.Sns.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Sns/)                         |
| [**MinimalLambda.Envelopes.Kinesis**](./MinimalLambda.Envelopes.Kinesis/README.md)                 | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Kinesis)                 | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Kinesis/)                 |
| [**MinimalLambda.Envelopes.KinesisFirehose**](./MinimalLambda.Envelopes.KinesisFirehose/README.md) | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.KinesisFirehose) | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.KinesisFirehose/) |
| [**MinimalLambda.Envelopes.Kafka**](./MinimalLambda.Envelopes.Kafka/README.md)                     | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Envelopes.Kafka.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Kafka)                     | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Envelopes.Kafka.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Kafka/)                     |
| [**MinimalLambda.Envelopes.CloudWatchLogs**](./MinimalLambda.Envelopes.CloudWatchLogs/README.md)   | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Envelopes.CloudWatchLogs.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.CloudWatchLogs)   | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Envelopes.CloudWatchLogs.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.CloudWatchLogs/)   |
| [**MinimalLambda.Envelopes.Alb**](./MinimalLambda.Envelopes.Alb/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/MinimalLambda.Envelopes.Alb.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Alb)                         | [![Downloads](https://img.shields.io/nuget/dt/MinimalLambda.Envelopes.Alb.svg)](https://www.nuget.org/packages/MinimalLambda.Envelopes.Alb/)                         |

Each package has detailed documentation in its own README file.

## Quick Start

Install the NuGet package:

```bash
dotnet add package MinimalLambda
```

Create a simple Lambda handler with dependency injection:

```csharp
using MinimalLambda.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Configure and run
var builder = LambdaApplication.CreateBuilder();
builder.Services.AddScoped<IGreetingService, GreetingService>();

var lambda = builder.Build();

// Inject the service directly into the handler
lambda.MapHandler(([Event] string input, IGreetingService greeting) => greeting.Greet(input));

await lambda.RunAsync();

// Define a service interface
public interface IGreetingService
{
    string Greet(string name);
}

// Implement the service
public class GreetingService : IGreetingService
{
    public string Greet(string name) => $"Hello {name}!";
}
```

See the [examples directory](./examples/) for more complete examples, including middleware and
OpenTelemetry integration.

## Documentation

- [MinimalLambda](./src/MinimalLambda/README.md) – Core framework documentation
- [MinimalLambda.Abstractions](./src/MinimalLambda.Abstractions/README.md) – Interfaces and
  abstractions
- [MinimalLambda.OpenTelemetry](./src/MinimalLambda.OpenTelemetry/README.md) – Observability
  documentation
- [Examples](./examples/) – Sample Lambda functions demonstrating framework features

## Contributing

Contributions are welcome! Bug reports, suggestions, and pull requests are all appreciated. Please
check the GitHub repository for guidelines.

## License

This project is licensed under the MIT License. See [LICENSE](./LICENSE) for details.
