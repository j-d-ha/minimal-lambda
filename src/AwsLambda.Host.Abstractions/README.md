# AwsLambda.Host.Abstractions

> ⚠️ **Development Status**: This project is actively under development and not yet
> production-ready. Breaking changes may occur in future versions. Use at your own discretion in
> production environments.

Core interfaces and delegates that define the aws-lambda-host framework contract. This package is
typically used implicitly by [AwsLambda.Host](../AwsLambda.Host/README.md), but useful if you're
building custom integrations, middleware, or extensions.

## Packages

The framework is divided into focused packages:

| Package                                                                       | NuGet                                                                                                                                    | Downloads                                                                                                                                      |
|-------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------|
| [**AwsLambda.Host**](../AwsLambda.Host/README.md)                             | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.svg)](https://www.nuget.org/packages/AwsLambda.Host)                             | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.svg)](https://www.nuget.org/packages/AwsLambda.Host/)                             |
| [**AwsLambda.Host.Abstractions**](../AwsLambda.Host.Abstractions/README.md)   | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Abstractions.svg)](https://www.nuget.org/packages/AwsLambda.Host.Abstractions)   | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Abstractions.svg)](https://www.nuget.org/packages/AwsLambda.Host.Abstractions/)   |
| [**AwsLambda.Host.OpenTelemetry**](../AwsLambda.Host.OpenTelemetry/README.md) | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.OpenTelemetry.svg)](https://www.nuget.org/packages/AwsLambda.Host.OpenTelemetry) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.OpenTelemetry.svg)](https://www.nuget.org/packages/AwsLambda.Host.OpenTelemetry/) |

## Installation

Install via NuGet:

```bash
dotnet add package AwsLambda.Host.Abstractions
```

Or specify a version:

```bash
dotnet add package AwsLambda.Host.Abstractions --version <version>
```

Ensure your project uses C# 11 or later:

```xml

<PropertyGroup>
  <LangVersion>11</LangVersion>
  <!-- or <LangVersion>latest</LangVersion> -->
</PropertyGroup>
```

> **Note:** This package is typically included automatically when you use
> [AwsLambda.Host](../AwsLambda.Host/README.md). Direct installation is only necessary when
> building custom integrations or extensions.

## Core Abstractions

### ILambdaApplication

The main builder interface for configuring a Lambda application using a fluent pattern:

- `MapHandler()` – Register the Lambda invocation handler
- `Use()` – Add middleware to the pipeline
- `OnInit()` – Register initialization handlers
- `OnShutdown()` – Register shutdown handlers

See [AwsLambda.Host](../AwsLambda.Host/README.md) for usage examples.

### ILambdaHostContext

Encapsulates a single Lambda invocation:

- `Event` – The deserialized Lambda event
- `Response` – The handler's response to return
- `ServiceProvider` – Access to the scoped DI container
- `Items` – Key/value collection for invocation-scoped data
- `CancellationToken` – Cancellation signal from Lambda timeout

### Handler Delegates

**LambdaInvocationDelegate**

```csharp
Task LambdaInvocationDelegate(ILambdaHostContext context)
```

Processes a Lambda invocation.

**LambdaInitDelegate**

```csharp
Task<bool> LambdaInitDelegate(IServiceProvider services, CancellationToken cancellationToken)
```

Runs once during initialization. Return `true` to continue, `false` to abort.

**LambdaShutdownDelegate**

```csharp
Task LambdaShutdownDelegate(IServiceProvider services, CancellationToken cancellationToken)
```

Runs once during shutdown for cleanup.

## Lambda Lifecycle

The abstractions represent three Lambda execution phases:

- **Init** – `LambdaInitDelegate` runs once during function initialization
- **Invocation** – `LambdaInvocationDelegate` runs for each event
- **Shutdown** – `LambdaShutdownDelegate` runs once before termination

For implementation details and examples, see [AwsLambda.Host](../AwsLambda.Host/README.md).

## Installation

Install via NuGet:

```bash
dotnet add package AwsLambda.Host.Abstractions
```

Or specify a version:

```bash
dotnet add package AwsLambda.Host.Abstractions --version <version>
```

Ensure your project uses C# 11 or later:

```xml

<PropertyGroup>
  <LangVersion>11</LangVersion>
  <!-- or <LangVersion>latest</LangVersion> -->
</PropertyGroup>
```

> **Note:** This package is typically included automatically when you use
> [AwsLambda.Host](../AwsLambda.Host/README.md). Direct installation is only necessary when
> building custom integrations or extensions.

## Related Packages

- **[AwsLambda.Host](../AwsLambda.Host/README.md)** – Core Lambda framework with middleware and DI
- **[AwsLambda.Host.OpenTelemetry](../AwsLambda.Host.OpenTelemetry/README.md)** – OpenTelemetry
  integration for distributed tracing
- **[Root README](../../README.md)** – Project overview and examples

## Documentation

- **[Full Project Documentation](https://github.com/j-d-ha/aws-lambda-host/wiki)** – Comprehensive
  guides and patterns
- **[Examples](../../examples/)** – Sample Lambda functions demonstrating framework patterns

## Contributing

Contributions are welcome! Please check the GitHub repository for contribution guidelines.

## License

This project is licensed under the MIT License. See [LICENSE](../../LICENSE) for details.
