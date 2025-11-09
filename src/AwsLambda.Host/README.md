# AwsLambda.Host

> ⚠️ **Development Status**: This project is actively under development and not yet
> production-ready. Breaking changes may occur in future versions. Use at your own discretion in
> production environments.

## Overview

**AwsLambda.Host** is the core framework package for building AWS Lambda functions using familiar
ASP.NET Core patterns. It provides a complete hosting experience with dependency injection,
middleware support, async/await patterns, and proper Lambda lifecycle management. Built on
Microsoft.Extensions generic host, it simplifies Lambda development while maintaining high
performance and AOT readiness.

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
dotnet add package AwsLambda.Host
```

Create a simple Lambda handler:

```csharp
using AwsLambda.Host;

var builder = LambdaApplication.CreateBuilder();
var lambda = builder.Build();

lambda.MapHandler(([Event] string input) => $"Hello {input}!");

await lambda.RunAsync();
```

## How It Works

### Complete Event Flow

When a Lambda function executes, here's what happens end-to-end:

1. **AWS Lambda Runtime** receives an event and invokes your application
2. **Event Deserialization** – The JSON event is deserialized into your event type (e.g., `string`,
   `OrderEvent`, etc.)
3. **Middleware Pipeline** – Each registered middleware executes in order, wrapping the handler
4. **Dependency Injection** – Services and the event are injected into your handler based on
   parameters
5. **Handler Execution** – Your handler logic runs with all dependencies available
6. **Response Serialization** – The handler's return value is serialized back to JSON
7. **Lambda Runtime** returns the response

This flow happens for each invocation while the container remains warm.

### Source Generation & Compile-Time Interception

The framework uses **source generators** to eliminate reflection and runtime overhead:

```csharp
lambda.MapHandler(
    ([Event] Order order, IOrderService service) =>
    {
        return service.Process(order);
    }
);
```

Instead of using reflection to inspect parameters and resolve dependencies at runtime, the
framework's source generator:

1. **Analyzes** your handler signature at compile time
2. **Identifies** parameters: `Order` (marked with `[Event]`), `IOrderService` (from DI)
3. **Generates** optimized code that directly instantiates dependencies and calls your handler
4. **Zero Runtime Cost** – No reflection, no dynamic invocation, no performance penalty

This design choice has major benefits:

- **Performance** – No reflection overhead, faster startup
- **AOT Ready** – Works with .NET Native AOT compilation for maximum Lambda performance
- **Type Safe** – All dependencies resolved and type-checked at compile time
- **Linker Friendly** – Trimming unused code is safe when everything is known at compile time

### Middleware & Handler Composition

The middleware pipeline is a classical "nesting" pattern where each middleware wraps the next:

```
Request
  ↓
[Middleware 1] ─┐
                ├→ [Middleware 2] ─┐
                                  ├→ [Handler] ← Innermost layer
                ← Returns ─────────┤
                ├───────────────────┤
  ↑ Response
```

When you register middleware and handlers:

```csharp
lambda.Use((context, next) => { /* Middleware 1 */ await next(); });
lambda.Use((context, next) => { /* Middleware 2 */ await next(); });
lambda.MapHandler(async (context) => { /* Handler */ });
```

The execution order is:

1. Middleware 1 pre-handler logic
2. Middleware 2 pre-handler logic
3. Handler executes
4. Middleware 2 post-handler logic (in finally block)
5. Middleware 1 post-handler logic (in finally block)

This enables middleware to:

- **Wrap** handler execution with try/catch for error handling
- **Inspect** the event before the handler runs
- **Modify** the response after the handler completes
- **Manage** resources with try/finally patterns

### Dependency Injection Scoping in Lambda

Understanding service lifetimes is critical for Lambda:

- **Singleton Services** – Created once per container, reused across all invocations
  - *Use for*: Connection pools, caches, configuration, expensive-to-create objects
  - *Avoid*: Request-scoped state, per-invocation data

- **Scoped Services** – Created once per invocation, disposed when invocation completes
  - *Use for*: Request context, correlation IDs, transaction scopes
  - *Default for*: Most application services

- **Transient Services** – Created each time requested
  - *Use for*: Stateless utilities, lightweight operations
  - *Avoid*: Most cases; usually scoped is better

The framework automatically creates a new scope for each invocation, ensuring scoped services are
properly isolated and disposed.

### Lambda Lifecycle Integration

The framework directly manages Lambda's execution model:

**Init Phase** (runs once):

- Your `OnInit()` handlers execute
- Long-running setup is performed once and reused
- Respects `InitTimeout` – if initialization takes too long, Lambda aborts

**Invocation Phase** (runs many times):

- Handler receives the deserialized event
- Scoped DI container created for this invocation
- Cancellation token automatically expires before Lambda timeout
- Response serialized and returned

**Shutdown Phase** (runs once):

- Your `OnShutdown()` handlers execute
- Resources are cleaned up gracefully
- Respects `ShutdownDuration` – limited time before hard termination

This mirrors how you'd structure a traditional service, but optimized for Lambda's stateless,
ephemeral nature.

## Core Concepts

### Lambda Lifecycle in Detail

Lambda execution consists of three phases, each with specific purposes:

**Init Phase** – Runs once when Lambda initializes the function (or resumes from Snap Start)

- Register initialization logic with `OnInit()`
- Ideal for opening database connections, loading large models, initializing caches, etc.
- Returns `bool`: `true` to continue, `false` to abort startup
- Respects `InitTimeout` setting – increase this for complex initialization
- Singleton services are created during this phase

**Invocation Phase** – Runs for each incoming event

- Handler executes within `MapHandler()`, potentially multiple times on the same container
- Each invocation gets its own scoped DI container for isolation
- Respects Lambda timeout minus `InvocationCancellationBuffer` for graceful cancellation
- Scoped services are created fresh for each invocation
- Middleware pipeline executes for each invocation

**Shutdown Phase** – Runs once before Lambda container terminates

- Register cleanup logic with `OnShutdown()`
- Ideal for closing connections, flushing metrics, saving state, etc.
- Respects `ShutdownDuration` setting – has limited time before hard container kill
- Critical for external extensions (OpenTelemetry exporters, APM agents)

See [AWS Lambda Runtime Environment](https://docs.aws.amazon.com/lambda/latest/dg/lambda-runtime-environment.html)
for more details on Lambda's execution model.

### Dependency Injection & Scoping

Register services in the builder and they're available to handlers, middleware, and lifecycle
handlers:

```csharp
var builder = LambdaApplication.CreateBuilder();

// Singletons persist across invocations (created once, reused many times)
builder.Services.AddSingleton<ICache, MemoryCache>();
builder.Services.AddSingleton<IConfiguration>(config);

// Scoped services are new per invocation (isolated per request)
builder.Services.AddScoped<IRequestContext, RequestContext>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Transients are created each time requested (rare in Lambda)
builder.Services.AddTransient<ILogger, ConsoleLogger>();
```

**Performance Tip:** Singletons are crucial for Lambda performance. Open database connections during
init (singleton), reuse them across invocations, close them during shutdown. This avoids the
overhead of connecting/disconnecting for every request.

Access services in handlers using the `[Event]` attribute for the event and normal parameter
injection for services:

```csharp
lambda.MapHandler(
    ([Event] MyEvent input, ICache cache, IOrderRepository repository) =>
    {
        var cached = cache.Get(input.Key);
        if (cached == null)
        {
            cached = repository.GetOrder(input.OrderId);
            cache.Set(input.Key, cached);
        }
        return cached;
    }
);
```

### Middleware Pipeline

Middleware enables cross-cutting concerns – logic that applies to every invocation. The pipeline is
built by composing middleware functions that wrap the handler:

```csharp
var lambda = builder.Build();

// First registered middleware
lambda.Use(async (context, next) =>
{
    Console.WriteLine("Middleware 1 - Before");
    try
    {
        await next(); // Call the next middleware/handler
    }
    finally
    {
        Console.WriteLine("Middleware 1 - After");
    }
});

// Second registered middleware
lambda.Use(async (context, next) =>
{
    Console.WriteLine("Middleware 2 - Before");
    try
    {
        await next(); // Call the next middleware/handler
    }
    finally
    {
        Console.WriteLine("Middleware 2 - After");
    }
});

// Handler always runs last
lambda.MapHandler(async (ILambdaHostContext context) =>
{
    Console.WriteLine("Handler");
    context.Response = "Done";
});
```

Output per invocation:

```
Middleware 1 - Before
Middleware 2 - Before
Handler
Middleware 2 - After
Middleware 1 - After
```

Common middleware patterns:

- **Request/Response Validation** – Validate input before handler, validate output after
- **Error Handling** – Wrap handler in try/catch, transform exceptions to responses
- **Logging & Correlation** – Extract correlation ID from event, add to context
- **Authorization** – Check permissions before allowing handler execution
- **Telemetry** – Record metrics, timings, and distributed traces
- **Resource Management** – Acquire resources before handler, release after

## Common Patterns

### Handler with Dependency Injection

Combine the fluent builder with parameter injection to keep handlers clean:

```csharp
var builder = LambdaApplication.CreateBuilder();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IDatabase, PostgresDatabase>();
builder.Services.AddSingleton<ICache, MemoryCache>();

var lambda = builder.Build();

lambda.MapHandler(
    ([Event] Order order, IOrderService service, IDatabase db, ICache cache) =>
    {
        // All dependencies injected automatically at compile time
        return service.Process(order, db, cache);
    }
);

await lambda.RunAsync();
```

This pattern keeps handlers focused on business logic while the framework handles dependency
management.

### Initialization & Resource Pooling

Use `OnInit()` to set up expensive resources once per container, and `OnShutdown()` to clean them
up:

```csharp
lambda.OnInit(async (services, cancellationToken) =>
{
    var db = services.GetRequiredService<IDatabase>();
    try
    {
        // Open connection during init - it will be reused for all invocations
        await db.ConnectAsync(cancellationToken);

        // Run any one-time setup
        await db.InitializeSchemaAsync(cancellationToken);

        return true; // Success
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Initialization failed: {ex}");
        return false; // Abort startup
    }
});

lambda.OnShutdown(async (services, cancellationToken) =>
{
    var db = services.GetRequiredService<IDatabase>();

    // Clean up the connection pool gracefully
    await db.CloseAsync(cancellationToken);
});
```

This is critical for performance: maintaining a single database connection across hundreds or
thousands of invocations is far cheaper than opening/closing for each request.

### Error Handling Middleware

Wrap handlers with exception handling to transform exceptions into appropriate responses:

```csharp
lambda.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (ValidationException ex)
    {
        context.Response = new ErrorResponse
        {
            StatusCode = 400,
            Message = ex.Message,
            Errors = ex.ValidationErrors
        };
    }
    catch (NotFoundException ex)
    {
        context.Response = new ErrorResponse
        {
            StatusCode = 404,
            Message = ex.Message
        };
    }
    catch (Exception ex)
    {
        // Log the error for debugging
        Console.Error.WriteLine($"Unhandled exception: {ex}");

        context.Response = new ErrorResponse
        {
            StatusCode = 500,
            Message = "Internal server error"
        };
    }
});
```

This ensures unhandled exceptions don't crash the invocation – they're transformed into structured
error responses.

### Type-Safe Context Access

Access the event and response with compile-time type safety:

```csharp
lambda.Use(async (context, next) =>
{
    // Safe type access - returns null if type mismatch
    if (context.GetEvent<OrderEvent>() is OrderEvent orderEvent)
    {
        // We know it's an OrderEvent
        if (orderEvent.OrderId <= 0)
        {
            context.Response = new ErrorResponse { Message = "Invalid order ID" };
            return;
        }

        await next(); // Process valid order
    }
    else
    {
        context.Response = new ErrorResponse { Message = "Invalid event type" };
    }
});
```

This pattern is useful in middleware that validates event types or handles multiple event types with
different logic.

### Clearing Lambda Output Formatting

Lambda's runtime adds text formatting to stdout (timestamps, request IDs). For structured logging (
JSON output), clear this formatting:

```csharp
var builder = LambdaApplication.CreateBuilder();

// Clear Lambda's text formatting on init - one-time operation
lambda.OnInitClearLambdaOutputFormatting();

// Then configure structured logging
builder.Logging.AddConsole(); // Now outputs clean JSON

// Or configure via options
builder.Services.ConfigureLambdaHostOptions(opts =>
{
    opts.ClearLambdaOutputFormatting = true;
});
```

This is essential when using Serilog or other structured logging libraries – without this, your JSON
logs get prefixed with Lambda's formatting.

## Cancellation & Timeouts

The framework automatically manages Lambda timeouts with cancellation tokens. This ensures graceful
shutdown instead of hard timeout kills:

Lambda invocation flow:

- Lambda timeout setting (e.g., 30 seconds)
- Minus `InvocationCancellationBuffer` (default 3 seconds)
- = Deadline when your cancellation token will be cancelled

```csharp
lambda.MapHandler(
    async ([Event] MyEvent input, IService service, CancellationToken cancellationToken) =>
    {
        // Cancellation token will be cancelled 3 seconds before Lambda timeout
        // Gives your code time to clean up gracefully
        return await service.LongRunningOperationAsync(input, cancellationToken);
    }
);
```

If your operation takes too long and the token is cancelled, you can handle it gracefully:

```csharp
try
{
    return await service.LongOperationAsync(cancellationToken);
}
catch (OperationCanceledException)
{
    // Timeout approaching - save state and return partial results
    return new PartialResponse { Status = "Timeout" };
}
```

Adjust the buffer based on your needs:

```csharp
builder.Services.ConfigureLambdaHostOptions(opts =>
{
    // Give more time for cleanup before hard timeout
    opts.InvocationCancellationBuffer = TimeSpan.FromSeconds(5);
});
```

Shorter buffers mean less cleanup time; longer buffers mean less time for your operation but more
time to gracefully finish.

## AOT (Ahead-of-Time) Compilation

The framework works natively with .NET Native AOT. To use AOT, you need to define a JSON serializer
context with source generation:

### Setting Up AOT

Enable AOT in your project file:

```xml

<PublishAot>true</PublishAot>
<PublishTrimmed>true</PublishTrimmed>
<InterceptorsNamespaces>$(InterceptorsNamespaces);AwsLambda.Host</InterceptorsNamespaces>
```

### Defining the Serializer Context

Create a `JsonSerializerContext` decorated with `[JsonSerializable]` attributes for each type your
handlers accept:

```csharp
using System.Text.Json.Serialization;

[JsonSerializable(typeof(Order))]
[JsonSerializable(typeof(OrderResponse))]
public partial class SerializerContext : JsonSerializerContext;

public record Order(int OrderId, string CustomerName);
public record OrderResponse(bool Success, string Message);
```

The `[JsonSerializable]` attribute tells the JSON source generator to create compile-time metadata
for those types. The class must be `partial` so the generator can extend it.

### Registering the Serializer

Configure the Lambda host to use your serializer context:

```csharp
var builder = LambdaApplication.CreateBuilder();

builder.Services.ConfigureLambdaHostOptions(options =>
    options.LambdaSerializer = new SourceGeneratorLambdaJsonSerializer<SerializerContext>()
);

var lambda = builder.Build();

lambda.MapHandler(([Event] Order order) =>
    new OrderResponse(Success: true, Message: $"Processed order {order.OrderId}")
);

await lambda.RunAsync();

// Event types that must be serializable
public record Order(int OrderId, string CustomerName);
public record OrderResponse(bool Success, string Message);

// Serializer context with all types registered
[JsonSerializable(typeof(Order))]
[JsonSerializable(typeof(OrderResponse))]
public partial class SerializerContext : JsonSerializerContext;
```

The generic type parameter specifies which `JsonSerializerContext` the serializer uses for all JSON
operations. Every type passed to `[JsonSerializable]` becomes available for JSON serialization at
runtime with AOT.

### What You Need to Know

- **All event types must be declared** – Each type used with `[Event]` must have a corresponding
  `[JsonSerializable]` attribute in your context
- **Public properties only** – AOT needs to see property definitions; use `[JsonPropertyName]` for
  property mapping if needed
- **No polymorphism** – Avoid abstract base classes or interfaces as event types; use concrete types
  instead
- **Dynamic DI is not supported** – Services must be registered at startup, not discovered at
  runtime
- **Generated code does the work** – The JSON source generator creates optimized serialization code
  at compile time; at runtime, there's no reflection

## Configuration

### LambdaHostOptions

Configure the Lambda host behavior through `LambdaHostOptions`. Set these via code or
`appsettings.json`:

```csharp
var builder = LambdaApplication.CreateBuilder();
builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.InitTimeout = TimeSpan.FromSeconds(10);
    options.InvocationCancellationBuffer = TimeSpan.FromSeconds(5);
    options.ShutdownDuration = ShutdownDuration.ExternalExtensions;
});
```

Or via **appsettings.json**:

```json
{
  "AwsLambdaHost": {
    "InitTimeout": "00:00:10",
    "InvocationCancellationBuffer": "00:00:05",
    "ShutdownDuration": "00:00:02"
  }
}
```

### Available Options

| Option                         | Type              | Default                     | Description                                                                                                                                                                                                                                                               |
|--------------------------------|-------------------|-----------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `InitTimeout`                  | TimeSpan          | 5s                          | Maximum time for `OnInit()` handlers to complete. Increase for complex initialization (e.g., database migrations, warming up caches).                                                                                                                                     |
| `InvocationCancellationBuffer` | TimeSpan          | 3s                          | Time subtracted from Lambda timeout to create cancellation deadline. Allows graceful shutdown before hard timeout. Increase if you need more cleanup time.                                                                                                                |
| `ShutdownDuration`             | TimeSpan          | 500ms                       | Grace period between Lambda shutdown signal (SIGTERM) and termination. Use constants: `ShutdownDuration.NoExtensions` (0ms), `ShutdownDuration.InternalExtensions` (300ms), `ShutdownDuration.ExternalExtensions` (500ms). Increase if you have slow external extensions. |
| `ShutdownDurationBuffer`       | TimeSpan          | 50ms                        | Additional buffer to ensure cleanup completes before container exit. Rarely needs adjustment.                                                                                                                                                                             |
| `ClearLambdaOutputFormatting`  | bool              | false                       | Clear Lambda runtime's text formatting on init. Set to `true` when using structured logging (Serilog, Log4net).                                                                                                                                                           |
| `LambdaSerializer`             | ILambdaSerializer | DefaultLambdaJsonSerializer | Custom JSON serializer for event/response handling. Useful if you need different serialization rules (camelCase, custom converters).                                                                                                                                      |
| `BootstrapHttpClient`          | HttpClient?       | null                        | Custom HTTP client for Lambda runtime API calls. Useful for proxying, custom headers, or network configuration.                                                                                                                                                           |

## Related Packages

- **[AwsLambda.Host.Abstractions](../AwsLambda.Host.Abstractions/README.md)** – Core interfaces and
  type definitions used by this package
- **[AwsLambda.Host.OpenTelemetry](../AwsLambda.Host.OpenTelemetry/README.md)** – OpenTelemetry
  integration for distributed tracing and observability
- **[Root README](../../README.md)** – Project overview, features, and links to examples

## Contributing

Contributions are welcome! Please check the GitHub repository for contribution guidelines.

## License

This project is licensed under the MIT License. See [LICENSE](../../LICENSE) for details.
