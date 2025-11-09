# AwsLambda.Host.OpenTelemetry

> ⚠️ **Development Status**: This project is actively under development and not yet
> production-ready. Breaking changes may occur in future versions. Use at your own discretion in
> production environments.

## Overview

**AwsLambda.Host.OpenTelemetry** provides OpenTelemetry integration for the aws-lambda-host
framework. It enables distributed tracing, metrics collection, and observability for Lambda
functions with minimal configuration. Built on the OpenTelemetry SDK, it integrates seamlessly with
the Lambda lifecycle and supports exporting to standard observability backends.

## Packages

The framework is divided into focused packages:

| Package                                                                       | NuGet                                                                                                                                    | Downloads                                                                                                                                      |
|-------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------|
| [**AwsLambda.Host**](../AwsLambda.Host/README.md)                             | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.svg)](https://www.nuget.org/packages/AwsLambda.Host)                             | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.svg)](https://www.nuget.org/packages/AwsLambda.Host/)                             |
| [**AwsLambda.Host.Abstractions**](../AwsLambda.Host.Abstractions/README.md)   | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Abstractions.svg)](https://www.nuget.org/packages/AwsLambda.Host.Abstractions)   | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Abstractions.svg)](https://www.nuget.org/packages/AwsLambda.Host.Abstractions/)   |
| [**AwsLambda.Host.OpenTelemetry**](../AwsLambda.Host.OpenTelemetry/README.md) | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.OpenTelemetry.svg)](https://www.nuget.org/packages/AwsLambda.Host.OpenTelemetry) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.OpenTelemetry.svg)](https://www.nuget.org/packages/AwsLambda.Host.OpenTelemetry/) |

Each package has detailed documentation in its own README file.

## Quick Start

Install the NuGet package:

```bash
dotnet add package AwsLambda.Host.OpenTelemetry
```

Configure OpenTelemetry in your Lambda application:

```csharp
using AwsLambda.Host;
using AwsLambda.Host.OpenTelemetry;

var builder = LambdaApplication.CreateBuilder();
builder.AddLambdaOpenTelemetry();

var lambda = builder.Build();
lambda.MapHandler(([Event] string input) => $"Traced: {input}");

await lambda.RunAsync();
```

## Contributing

Contributions are welcome! Please check the GitHub repository for contribution guidelines.

## License

This project is licensed under the MIT License. See [LICENSE](../../LICENSE) for details.
