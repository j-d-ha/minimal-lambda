# MinimalLambda

[![Main Build](https://github.com/j-d-ha/minimal-lambda/actions/workflows/main-build.yaml/badge.svg)](https://github.com/j-d-ha/minimal-lambda/actions/workflows/main-build.yaml)
[![codecov](https://codecov.io/gh/j-d-ha/minimal-lambda/graph/badge.svg?token=BWORPTQ0UK)](https://codecov.io/gh/j-d-ha/minimal-lambda)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=j-d-ha_minimal-lambda&metric=alert_status&token=9fb519975d91379dcfbc6c13a4bd4207131af6e3)](https://sonarcloud.io/summary/new_code?id=j-d-ha_minimal-lambda)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A modern .NET framework for building AWS Lambda functions using familiar .NET patterns.

> 📚 **[View Full Documentation](https://j-d-ha.github.io/minimal-lambda/)**

## Overview

**minimal-lambda** is a .NET hosting framework that simplifies AWS Lambda development by following
idiomatic .NET patterns and best practices. Instead of writing boilerplate code to handle Lambda
events, context, and serialization, you get a clean, declarative API for defining Lambda handlers
with dependency injection (similar to minimal APIs), middleware support, and modern async/await
patterns. Built on the generic host from Microsoft.Extensions, it provides a .NET hosting experience
similar to ASP.NET Core but tailored specifically for Lambda.

## Key Features

- **.NET Hosting Patterns** – Use middleware, builder pattern, and dependency injection (similar to
  ASP.NET Core), with proper scoped lifetime management per invocation
- **Async-First Design** – Native support for async/await with proper Lambda timeout and
  cancellation handling
- **Source Generators & Interceptors** – Compile-time code generation and method interception for
  optimal performance
- **AOT Ready** – Support for Ahead-of-Time compilation for faster cold starts
- **Built-in Observability** – OpenTelemetry integration for distributed tracing
- **Flexible Handler Registration** – Simple, declarative API for mapping Lambda event types to
  handlers
- **Minimal Runtime Overhead** – No unnecessary abstractions; efficient use of Lambda resources

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
