# aws-lambda-host

[![Main Build](https://github.com/j-d-ha/aws-lambda-host/actions/workflows/main-build.yaml/badge.svg)](https://github.com/j-d-ha/aws-lambda-host/actions/workflows/main-build.yaml)
[![codecov](https://codecov.io/gh/j-d-ha/aws-lambda-host/graph/badge.svg?token=BWORPTQ0UK)](https://codecov.io/gh/j-d-ha/aws-lambda-host)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=j-d-ha_aws-lambda-host&metric=alert_status&token=9fb519975d91379dcfbc6c13a4bd4207131af6e3)](https://sonarcloud.io/summary/new_code?id=j-d-ha_aws-lambda-host)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

A modern .NET framework for building AWS Lambda functions using familiar .NET patterns.

## Overview

**aws-lambda-host** is a .NET hosting framework that simplifies AWS Lambda development by following
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

| Package                                                                          | NuGet                                                                                                                                    | Downloads                                                                                                                                      |
|----------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------|
| [**AwsLambda.Host**](./src/AwsLambda.Host/README.md)                             | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.svg)](https://www.nuget.org/packages/AwsLambda.Host)                             | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.svg)](https://www.nuget.org/packages/AwsLambda.Host/)                             |
| [**AwsLambda.Host.Abstractions**](./src/AwsLambda.Host.Abstractions/README.md)   | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Abstractions.svg)](https://www.nuget.org/packages/AwsLambda.Host.Abstractions)   | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Abstractions.svg)](https://www.nuget.org/packages/AwsLambda.Host.Abstractions/)   |
| [**AwsLambda.Host.OpenTelemetry**](./src/AwsLambda.Host.OpenTelemetry/README.md) | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.OpenTelemetry.svg)](https://www.nuget.org/packages/AwsLambda.Host.OpenTelemetry) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.OpenTelemetry.svg)](https://www.nuget.org/packages/AwsLambda.Host.OpenTelemetry/) |

### Envelopes Packages

Envelope packages provide specialized support for handling different AWS Lambda event sources,
including SQS, SNS, API Gateway, Kinesis, and more.

| Package                                                                                              | NuGet                                                                                                                                                            | Downloads                                                                                                                                                              |
|------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [**AwsLambda.Host.Envelopes.Sqs**](./AwsLambda.Host.Envelopes.Sqs/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs/)                         |
| [**AwsLambda.Host.Envelopes.ApiGateway**](./AwsLambda.Host.Envelopes.ApiGateway/README.md)           | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway)           | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway/)           |
| [**AwsLambda.Host.Envelopes.Sns**](./AwsLambda.Host.Envelopes.Sns/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns/)                         |
| [**AwsLambda.Host.Envelopes.Kinesis**](./AwsLambda.Host.Envelopes.Kinesis/README.md)                 | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kinesis)                 | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kinesis/)                 |
| [**AwsLambda.Host.Envelopes.KinesisFirehose**](./AwsLambda.Host.Envelopes.KinesisFirehose/README.md) | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.KinesisFirehose) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.KinesisFirehose/) |
| [**AwsLambda.Host.Envelopes.Kafka**](./AwsLambda.Host.Envelopes.Kafka/README.md)                     | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Kafka.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kafka)                     | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Kafka.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kafka/)                     |
| [**AwsLambda.Host.Envelopes.CloudWatchLogs**](./AwsLambda.Host.Envelopes.CloudWatchLogs/README.md)   | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.CloudWatchLogs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.CloudWatchLogs)   | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.CloudWatchLogs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.CloudWatchLogs/)   |
| [**AwsLambda.Host.Envelopes.Alb**](./AwsLambda.Host.Envelopes.Alb/README.md)                         | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb/)                         |

Each package has detailed documentation in its own README file.

## Quick Start

Install the NuGet package:

```bash
dotnet add package AwsLambda.Host
```

Create a simple Lambda handler with dependency injection:

```csharp
using AwsLambda.Host.Builder;
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

- [AwsLambda.Host](./src/AwsLambda.Host/README.md) – Core framework documentation
- [AwsLambda.Host.Abstractions](./src/AwsLambda.Host.Abstractions/README.md) – Interfaces and
  abstractions
- [AwsLambda.Host.OpenTelemetry](./src/AwsLambda.Host.OpenTelemetry/README.md) – Observability
  documentation
- [Examples](./examples/) – Sample Lambda functions demonstrating framework features

## Contributing

Contributions are welcome! Bug reports, suggestions, and pull requests are all appreciated. Please
check the GitHub repository for guidelines.

## License

This project is licensed under the MIT License. See [LICENSE](./LICENSE) for details.
