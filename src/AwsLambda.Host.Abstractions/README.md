# AwsLambda.Host.Abstractions

Core interfaces and abstractions for the aws-lambda-host framework.

> ⚠️ **Development Status**: This project is actively under development and not yet
> production-ready. Breaking changes may occur in future versions. Use at your own discretion in
> production environments.

## Overview

Core interfaces and delegates that define the AwsLambda.Host framework contract. This package
provides:

- **Handler Abstractions**: Interfaces for Lambda request and response handling across different
  event types
- **Envelope Abstractions**: Contracts for extracting request payloads and packing response payloads
- **Middleware Contracts**: Abstractions for building and composing middleware components
- **Dependency Injection**: Service container and lifetime management interfaces
- **Extension Points**: Contracts for custom integrations and framework extensions

This package is typically used implicitly by [AwsLambda.Host](../AwsLambda.Host/README.md), but is
essential if you're building custom integrations, middleware components, or extensions to the
framework.

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

### Builder Interfaces

The framework uses three specialized builder interfaces for configuring different Lambda execution phases, providing clear separation of concerns:

**ILambdaInvocationBuilder**

Configures the Lambda invocation request/response pipeline:

- `Handle(LambdaInvocationDelegate handler)` – Register the Lambda invocation handler that processes
  each incoming event
- `Use(Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware)` – Add middleware to the
  invocation pipeline; middleware is composed sequentially and can inspect/modify the context before
  and after invocation
- `Handler` (property) – The currently registered invocation handler
- `Middlewares` (property) – Collection of registered middleware components
- `Properties` (property) – Key/value collection for sharing between the builder phases and all
  invocations
- `Services` (property) – `IServiceProvider` for accessing registered services during configuration
- `Build()` – Compiles the configured handler and middleware into an executable
  `LambdaInvocationDelegate`

**ILambdaOnInitBuilder**

Configures the initialization phase (runs once on cold start):

- `OnInit(LambdaInitDelegate handler)` – Register initialization handlers that run before the first
  invocation; handlers are executed and return `true` to proceed or `false` to abort initialization
- `InitHandlers` (property) – Collection of registered initialization handlers
- `Services` (property) – `IServiceProvider` for accessing registered services during configuration
- `Build()` – Compiles the configured handlers into an executable initialization delegate with
  concurrent execution and error aggregation

**ILambdaOnShutdownBuilder**

Configures the shutdown phase (runs once before Lambda termination):

- `OnShutdown(LambdaShutdownDelegate handler)` – Register shutdown handlers that run during cleanup;
  handlers execute sequentially for graceful resource release
- `ShutdownHandlers` (property) – Collection of registered shutdown handlers
- `Services` (property) – `IServiceProvider` for accessing registered services during configuration
- `Build()` – Compiles the configured handlers into an executable shutdown delegate with sequential
  execution and error logging

These interfaces are obtained from `LambdaApplication` after calling `Build()`. The builder pattern flow is:

```
LambdaApplication.CreateBuilder()
  → Configure services
    → .Build()
      → Returns LambdaApplication (implementing all three builder interfaces)
        → Configure invocation pipeline (ILambdaInvocationBuilder)
        → Configure init handlers (ILambdaOnInitBuilder)
        → Configure shutdown handlers (ILambdaOnShutdownBuilder)
```

This design separates concerns between request/response handling, initialization, and lifecycle cleanup. See [AwsLambda.Host](../AwsLambda.Host/README.md) for detailed usage examples and the complete builder API.

### ILambdaHostContext

Encapsulates a single Lambda invocation and provides access to contextual information and services:

- `ServiceProvider` – Access to the scoped DI container for resolving services during invocation
- `CancellationToken` – Cancellation signal triggered when Lambda approaches its timeout, allowing
  graceful shutdown
- `Features` – `IFeatureCollection` providing access to custom functionalaty within the invocation
  pipeline such as for accessing the invocation Event or Response data
- `Items` – `IDictionary<object, object?>` for storing invocation-scoped data; cleared per
  invocation
- `Properties` – `IDictionary<string, object?>` for accessing shared data configured during the
  build phase; persists across invocations
- `RawInvocationData` – Raw stream access to serialized event and response data via
  `RawInvocationData.Event` and `RawInvocationData.Response`

#### Properties vs Items

`Properties` and `Items` serve different purposes:

- **Properties**: Configured during the build phase and available for the lifetime of the Lambda
  function. Use this for data that should be shared across invocations (e.g., configuration values
  set during initialization).
- **Items**: Scoped to a single invocation and cleared after each request completes. Use this for
  temporary state that is specific to a single Lambda invocation.

### ILambdaCancellationFactory

Provides a factory for creating cancellation token sources configured for AWS Lambda invocations:

- `NewCancellationTokenSource(ILambdaContext)` – Creates a `CancellationTokenSource` that cancels
  before the Lambda function timeout, allowing time for graceful shutdown

This interface enables custom implementations for managing cancellation tokens with respect to
Lambda's remaining execution time. The default implementation applies a configurable buffer duration
to ensure operations complete before the Lambda runtime's hard timeout.

### Envelope Abstractions

**IRequestEnvelope**

Defines a contract for extracting and deserializing incoming Lambda event payloads. Implementations
extract the inner payload from the outer Lambda event structure and deserialize it for handler
processing.

**IResponseEnvelope**

Defines a contract for serializing and packing handler results into Lambda response structures.
Implementations serialize the handler result and place it in the appropriate location within the
outer response structure.

These abstractions enable strongly-typed handling of AWS Lambda events (like API Gateway, SQS) with
automatic payload extraction and response packing. See the envelope packages for concrete
implementations.

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
