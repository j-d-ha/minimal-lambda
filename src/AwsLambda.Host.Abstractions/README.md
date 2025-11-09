# AwsLambda.Host.Abstractions

> ⚠️ **Development Status**: This project is actively under development and not yet
> production-ready. Breaking changes may occur in future versions. Use at your own discretion in
> production environments.

## Overview

**AwsLambda.Host.Abstractions** provides the core interfaces and delegates that define the contract
for the aws-lambda-host framework. This package contains the essential abstractions you'll work with
when building Lambda functions, including the handler builder pattern, invocation context, and
lifecycle delegates.

**Most developers using aws-lambda-host won't directly reference this package** — it's implicitly
used by [AwsLambda.Host](../AwsLambda.Host/README.md). However, it's useful if you're building
custom integrations, middleware, or extensions for the framework.

## Packages

The framework is divided into focused packages:

| Package                                                                       | NuGet                                                                                                                                    | Downloads                                                                                                                                      |
|-------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------|
| [**AwsLambda.Host**](../AwsLambda.Host/README.md)                             | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.svg)](https://www.nuget.org/packages/AwsLambda.Host)                             | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.svg)](https://www.nuget.org/packages/AwsLambda.Host/)                             |
| [**AwsLambda.Host.Abstractions**](../AwsLambda.Host.Abstractions/README.md)   | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Abstractions.svg)](https://www.nuget.org/packages/AwsLambda.Host.Abstractions)   | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Abstractions.svg)](https://www.nuget.org/packages/AwsLambda.Host.Abstractions/)   |
| [**AwsLambda.Host.OpenTelemetry**](../AwsLambda.Host.OpenTelemetry/README.md) | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.OpenTelemetry.svg)](https://www.nuget.org/packages/AwsLambda.Host.OpenTelemetry) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.OpenTelemetry.svg)](https://www.nuget.org/packages/AwsLambda.Host.OpenTelemetry/) |

Each package has detailed documentation in its own README file.

## Table of Contents

- [Packages](#packages)
- [Core Abstractions](#core-abstractions)
  - [ILambdaApplication](#ilambdaapplication)
  - [ILambdaHostContext](#ilambdahostcontext)
  - [Handler Delegates](#handler-delegates)
- [Lambda Lifecycle](#lambda-lifecycle)
- [Common Patterns](#common-patterns)
- [Installation](#installation)

## Core Abstractions

### ILambdaApplication

The main builder interface for configuring a Lambda application. It follows the fluent builder
pattern similar to ASP.NET Core Minimal APIs.

**Key Methods:**

- `MapHandler()` – Register the main Lambda invocation handler
- `Use()` – Add middleware to the invocation pipeline
- `OnInit()` – Register startup handlers during the Lambda init phase
- `OnShutdown()` – Register shutdown handlers during the Lambda shutdown phase

**Example Pattern:**

```csharp
var builder = LambdaApplication.CreateBuilder();
builder.Services.AddScoped<IMyService, MyService>();

var lambda = builder.Build();
lambda.MapHandler(async (context) => { /* handle invocation */ });
lambda.OnInit(InitializationHandler);

await lambda.RunAsync();
```

This builder pattern enables you to register handlers, add middleware, configure dependency
injection, and manage the Lambda lifecycle declaratively.

### ILambdaHostContext

Encapsulates all information about a single Lambda invocation. It extends AWS's standard
`ILambdaContext` and provides:

- `Event` – The deserialized Lambda event
- `Response` – The handler's response (to be serialized back)
- `ServiceProvider` – Access to the scoped dependency injection container
- `Items` – Key/value collection for invocation-scoped data sharing
- `CancellationToken` – Signals cancellation when Lambda timeout approaches

You'll receive this context in your handler, middleware, and throughout the invocation pipeline:

```csharp
lambda.MapHandler(async (ILambdaHostContext context) =>
{
    var input = context.Event as MyEventType;
    var service = context.ServiceProvider.GetRequiredService<IMyService>();
    context.Response = await service.ProcessAsync(input);
});
```

### Handler Delegates

Three delegate types manage the Lambda lifecycle:

**LambdaInvocationDelegate**

```csharp
Task LambdaInvocationDelegate(ILambdaHostContext context)
```

The core handler that processes Lambda invocations. Receives the invocation context and is
responsible for setting the response.

**LambdaStartupDelegate** (also called `LambdaInitDelegate`)

```csharp
Task<bool> LambdaInitDelegate(IServiceProvider services, CancellationToken cancellationToken)
```

Invoked during Lambda's init phase (before any handler invocation). Returns `true` to continue
initialization or `false` to abort. Useful for pre-initialization
during [Snap Start](https://docs.aws.amazon.com/lambda/latest/dg/snapstart.html).

**LambdaShutdownDelegate**

```csharp
Task LambdaShutdownDelegate(IServiceProvider services, CancellationToken cancellationToken)
```

Invoked during Lambda's shutdown phase. Use this for cleanup (closing connections, flushing metrics,
etc.). Multiple handlers can be registered.

## Lambda Lifecycle

Lambda executes in three distinct phases:

1. **Init Phase** – Runs once when the Lambda runtime initializes the function (or resumes from Snap
   Start). Initialization code (like opening database connections) runs here and is reused across
   invocations. Register handlers with `OnInit()`.
2. **Invocation Phase** – Runs for each incoming event. Your `MapHandler()` executes here,
   potentially multiple times for a single container. Each invocation is isolated with its own
   scoped dependency injection container.
3. **Shutdown Phase** – Runs once before the runtime shuts down the container. Use this for
   cleanup (closing connections, flushing metrics, etc.). Register handlers with `OnShutdown()`.

The abstractions in this package align with these phases. For detailed information about the
execution model and examples, see [AwsLambda.Host](../AwsLambda.Host/README.md).

**For more details on the AWS Lambda runtime environment, see
the [AWS Lambda Runtime Environment](https://docs.aws.amazon.com/lambda/latest/dg/lambda-runtime-environment.html)
documentation.**

## Common Patterns

**Using Dependency Injection in Handlers:**

```csharp
lambda.MapHandler(
    ([Event] MyEventType input, IMyService service) =>
    {
        return service.Process(input);
    }
);
```

The framework automatically injects dependencies registered in `builder.Services`.

**Middleware Pipeline:**

```csharp
lambda.Use(async (context, next) =>
{
    // Before invocation
    try
    {
        await next();
    }
    finally
    {
        // After invocation (cleanup)
    }
});
```

Middleware wraps the invocation handler, enabling cross-cutting concerns like logging, error
handling, and telemetry.

**Initialization & Cleanup:**

```csharp
lambda.OnInit(async (services, ct) =>
{
    var db = services.GetRequiredService<IDatabase>();
    await db.ConnectAsync(ct);
    return true; // success
});

lambda.OnShutdown(async (services, ct) =>
{
    var db = services.GetRequiredService<IDatabase>();
    await db.DisconnectAsync(ct);
});
```

## Installation

Install via NuGet:

```bash
dotnet add package AwsLambda.Host.Abstractions
```

Or specify a version:

```bash
dotnet add package AwsLambda.Host.Abstractions --version <version>
```

## Contributing

Contributions are welcome! Please check the GitHub repository for contribution guidelines.

## License

This project is licensed under the MIT License. See [LICENSE](../../LICENSE) for details.
